const http = require("http");
const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const root = path.resolve(__dirname, "..", "..");
const port = Number(process.env.PORT) || 8765;
const toolsDir = path.join(root, "tools");
const rollProject = path.join(toolsDir, "RollServer", "RollServer.csproj");
const rollOut = path.join(toolsDir, "RollServer", "bin", "server");
const coreProps = path.join(toolsDir, "RiftCore.props");

// Files the page may write. The marker keeps a stray post from replacing a file with something else.
const editable = {
	"/api/loot": { file: path.join(root, "Riftaria", "Common", "Drops", "loot-pools.js"), marker: "var LOOT_POOLS" },
	"/api/affixes": { file: path.join(root, "Riftaria", "Common", "Items", "affixes.js"), marker: "var AFFIXES" }
};

const types = {
	".html": "text/html; charset=utf-8",
	".js": "text/javascript; charset=utf-8",
	".css": "text/css; charset=utf-8"
};

function send(res, status, body, type) {
	res.writeHead(status, {
		"Content-Type": type || "text/plain; charset=utf-8",
		"Cache-Control": "no-store",
		"Access-Control-Allow-Origin": "*",
		"Access-Control-Allow-Methods": "GET, POST, OPTIONS",
		"Access-Control-Allow-Headers": "Content-Type"
	});
	res.end(body);
}

function readBody(req) {
	return new Promise((resolve, reject) => {
		const chunks = [];
		let size = 0;
		req.on("data", chunk => {
			size += chunk.length;
			if (size > 2_000_000) {
				reject(new Error("too large"));
				req.destroy();
				return;
			}
			chunks.push(chunk);
		});
		req.on("end", () => resolve(Buffer.concat(chunks).toString("utf8")));
		req.on("error", reject);
	});
}

function isData(text, marker) {
	if (!text.startsWith(marker)) {
		return false;
	}
	const start = text.indexOf("{");
	const end = text.lastIndexOf("}");
	if (start < 0 || end <= start) {
		return false;
	}
	try {
		JSON.parse(text.slice(start, end + 1));
		return true;
	}
	catch {
		return false;
	}
}

// ---- Roll server ----------------------------------------------------------
// Rolls and checks run in tools/RollServer, built from the mod's own C#, so the
// page cannot drift from the game. It is rebuilt when any of that C# changes.

let roller = null;
let starting = null;

function sourceFiles() {
	const files = [coreProps, rollProject];
	const props = fs.readFileSync(coreProps, "utf8");
	for (const match of props.matchAll(/Include="([^"]+)"/g)) {
		files.push(path.resolve(toolsDir, match[1].replace("$(MSBuildThisFileDirectory)", "")));
	}
	for (const name of fs.readdirSync(path.dirname(rollProject))) {
		if (name.endsWith(".cs")) {
			files.push(path.join(path.dirname(rollProject), name));
		}
	}
	return files;
}

function newestSource() {
	let newest = 0;
	for (const file of sourceFiles()) {
		try {
			newest = Math.max(newest, fs.statSync(file).mtimeMs);
		}
		catch {
			// A missing file fails the build, which reports it.
		}
	}
	return newest;
}

function build() {
	return new Promise((resolve, reject) => {
		const proc = spawn("dotnet", ["build", rollProject, "-c", "Release", "-o", rollOut, "-nologo", "-v", "q"], { cwd: root });
		let log = "";
		proc.stdout.on("data", chunk => log += chunk);
		proc.stderr.on("data", chunk => log += chunk);
		proc.on("error", error => reject(new Error("could not run dotnet: " + error.message)));
		proc.on("exit", code => {
			if (code === 0) {
				resolve();
				return;
			}
			const errors = log.split(/\r?\n/).filter(line => / error /.test(line));
			reject(new Error("RollServer did not build.\n" + [...new Set(errors)].slice(0, 8).join("\n")));
		});
	});
}

async function startRoller() {
	if (roller) {
		roller.proc.kill();
		roller = null;
	}
	// Stamped before the build, so an edit made while it builds triggers another.
	const stamp = Date.now();
	console.log("building RollServer...");
	await build();
	const proc = spawn("dotnet", [path.join(rollOut, "RollServer.dll")], { cwd: root });
	const instance = { proc, builtAt: stamp, pending: new Map(), nextId: 1, buffer: "" };
	proc.stdout.setEncoding("utf8");
	proc.stdout.on("data", chunk => {
		instance.buffer += chunk;
		let end;
		while ((end = instance.buffer.indexOf("\n")) >= 0) {
			const line = instance.buffer.slice(0, end).trim();
			instance.buffer = instance.buffer.slice(end + 1);
			if (!line.startsWith("{")) {
				continue;
			}
			let reply;
			try {
				reply = JSON.parse(line);
			}
			catch {
				continue;
			}
			const waiting = instance.pending.get(reply.id);
			if (waiting) {
				instance.pending.delete(reply.id);
				reply.ok ? waiting.resolve(reply.result) : waiting.reject(new Error(reply.error));
			}
		}
	});
	proc.stderr.on("data", chunk => process.stderr.write(chunk));
	proc.on("exit", () => {
		for (const waiting of instance.pending.values()) {
			waiting.reject(new Error("RollServer stopped."));
		}
		instance.pending.clear();
		if (roller === instance) {
			roller = null;
		}
	});
	roller = instance;
	console.log("RollServer ready");
	return instance;
}

// A failed build is not retried until the C# changes again.
let failed = null;

async function ensureRoller(force) {
	if (starting) {
		return starting;
	}
	const newest = newestSource();
	if (!force && roller && roller.builtAt >= newest) {
		return roller;
	}
	if (!force && failed && failed.newest === newest) {
		throw failed.error;
	}
	starting = startRoller();
	try {
		const instance = await starting;
		failed = null;
		return instance;
	}
	catch (error) {
		failed = { newest, error };
		console.error(error.message);
		throw error;
	}
	finally {
		starting = null;
	}
}

// A request with rebuild: true rebuilds and restarts RollServer first, even if nothing changed.
async function roll(request) {
	const instance = await ensureRoller(request.rebuild === true);
	const id = instance.nextId++;
	return new Promise((resolve, reject) => {
		instance.pending.set(id, { resolve, reject });
		instance.proc.stdin.write(JSON.stringify({ ...request, id }) + "\n");
	});
}

process.on("exit", () => roller && roller.proc.kill());
process.on("SIGINT", () => process.exit(0));

const server = http.createServer(async (req, res) => {
	const url = (req.url || "/").split("?")[0];
	if (req.method === "OPTIONS") {
		send(res, 204, "");
		return;
	}

	if (req.method === "POST" && url === "/api/roll") {
		try {
			const request = JSON.parse(await readBody(req));
			const result = await roll(request);
			send(res, 200, JSON.stringify(result), "application/json; charset=utf-8");
		}
		catch (error) {
			send(res, 500, JSON.stringify({ error: error.message }), "application/json; charset=utf-8");
		}
		return;
	}

	if (req.method === "POST") {
		const target = editable[url];
		if (!target) {
			send(res, 404, "missing");
			return;
		}

		let text = "";
		try {
			text = await readBody(req);
		}
		catch {
			send(res, 400, "could not read the file");
			return;
		}

		if (!isData(text, target.marker)) {
			send(res, 400, "not a " + path.basename(target.file) + " file");
			return;
		}

		if (fs.existsSync(target.file)) {
			fs.copyFileSync(target.file, target.file + ".bak");
		}
		fs.writeFileSync(target.file, text.endsWith("\n") ? text : text + "\n");
		send(res, 200, "saved");
		return;
	}

	if (req.method !== "GET") {
		send(res, 404, "missing");
		return;
	}

	let rel = decodeURIComponent(url).replace(/^\/+/, "");
	if (!rel) {
		rel = "tools/rift-preview/index.html";
	}
	const file = path.resolve(root, rel);
	if (!file.startsWith(root)) {
		send(res, 403, "forbidden");
		return;
	}

	fs.readFile(file, (error, body) => {
		if (error) {
			send(res, 404, "missing");
			return;
		}
		send(res, 200, body, types[path.extname(file)] || "text/plain; charset=utf-8");
	});
});

server.listen(port, "127.0.0.1", () => {
	console.log("ready http://127.0.0.1:" + port + "/tools/rift-preview/index.html");
	ensureRoller().catch(() => {});
});
