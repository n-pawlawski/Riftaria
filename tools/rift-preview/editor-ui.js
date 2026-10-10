// Shared state and small DOM helpers for the item tabs. Every tab edits the
// same objects, so the roll preview always sees unsaved edits.
//
// Game rules (rolls, checks, tier fits, weights) are never computed here: they
// come from tools/RollServer through server.js, built from the mod's own C#.
(function (global) {
	"use strict";

	// Opened from disk, the page still saves through server.js on its usual port.
	const SERVER = location.protocol.startsWith("http") ? location.origin : "http://127.0.0.1:8765";
	const FILES = {
		affixes: { url: "/api/affixes", variable: "AFFIXES", name: "affixes.js" },
		loot: { url: "/api/loot", variable: "LOOT_POOLS", name: "loot-pools.js" }
	};
	const RARITIES = ["normal", "magic", "rare"];

	function int(value, fallback) {
		const parsed = Number(value);
		return Number.isFinite(parsed) ? Math.trunc(parsed) : (fallback || 0);
	}

	function str(value, fallback) {
		return value === undefined || value === null ? (fallback || "") : String(value);
	}

	function list(value) {
		return Array.isArray(value) ? value : [];
	}

	// Fills in every field the editors touch, so they never meet a missing one.
	function normalizeAffixes(file) {
		file = file || {};
		const rarities = file.rarities || {};
		const names = file.rareNames || {};
		const count = entry => ({ count: int(entry.count), weight: int(entry.weight) });
		return {
			tags: list(file.tags).map(tag => str(tag)),
			stats: list(file.stats).map(stat => ({
				id: str(stat.id),
				label: str(stat.label),
				format: stat.format === "percent" ? "percent" : "flat",
				scope: stat.scope === "local" ? "local" : "global"
			})),
			rarities: { magic: list(rarities.magic).map(count), rare: list(rarities.rare).map(count) },
			rareNames: { first: list(names.first).map(word => str(word)), second: list(names.second).map(word => str(word)) },
			affixes: list(file.affixes).map(affix => ({
				id: str(affix.id),
				name: str(affix.name),
				kind: affix.kind === "suffix" ? "suffix" : "prefix",
				group: str(affix.group),
				stat: str(affix.stat),
				tags: list(affix.tags).map(tag => str(tag)),
				tiers: list(affix.tiers).map(tier => ({
					minLevel: int(tier.minLevel, 1),
					min: int(tier.min),
					max: int(tier.max),
					weight: int(tier.weight)
				}))
			}))
		};
	}

	function normalizeRarity(weights) {
		weights = weights || {};
		return { normal: int(weights.normal), magic: int(weights.magic), rare: int(weights.rare) };
	}

	function normalizeLoot(file) {
		file = file || {};
		return {
			bases: list(file.bases).map(entry => ({
				item: str(entry.item),
				tag: str(entry.tag),
				minTier: int(entry.minTier, 1),
				maxTier: int(entry.maxTier),
				weight: entry.weight === undefined ? 100 : int(entry.weight)
			})),
			tiers: list(file.tiers).map(tier => ({
				tier: int(tier.tier),
				general: {
					dropChance: Number(tier.general?.dropChance) || 0,
					baseCount: int(tier.general?.baseCount),
					rarity: normalizeRarity(tier.general?.rarity)
				},
				boss: {
					items: tier.boss?.items === undefined ? 1 : int(tier.boss.items),
					baseCount: int(tier.boss?.baseCount),
					rarity: normalizeRarity(tier.boss?.rarity)
				}
			})),
			bosses: list(file.bosses).map(entry => ({
				boss: str(entry.boss),
				tier: int(entry.tier),
				items: entry.items === null || entry.items === undefined ? null : int(entry.items),
				rarity: entry.rarity ? normalizeRarity(entry.rarity) : null
			}))
		};
	}

	const listeners = {};
	const state = {
		affixes: normalizeAffixes(global.AFFIXES),
		loot: normalizeLoot(global.LOOT_POOLS),
		dirty: { affixes: false, loot: false }
	};

	// Sends the current, possibly unsaved, files along with every request.
	async function api(op, payload) {
		let response;
		try {
			response = await fetch(SERVER + "/api/roll", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ ...payload, op, affixes: state.affixes, loot: state.loot })
			});
		}
		catch {
			throw new Error("Could not reach server.js. Start it with: node tools/rift-preview/server.js");
		}
		const body = await response.json().catch(() => ({ error: "server.js sent a reply that is not JSON." }));
		if (!response.ok) {
			throw new Error(body.error || "The roll server failed.");
		}
		return body;
	}

	// Stand-ins until the roll server answers.
	const meta = { stats: [], caps: { magic: 1, rare: 3 }, ready: false };
	const inspection = { affixErrors: [], lootErrors: [], tiers: [], affixWeights: [], offline: "", ready: false };

	async function loadMeta() {
		try {
			Object.assign(meta, await api("meta", {}), { ready: true });
			emit("meta");
		}
		catch {
			setTimeout(loadMeta, 3000);
		}
	}

	let inspectSerial = 0;
	async function inspect() {
		const serial = ++inspectSerial;
		try {
			const result = await api("inspect", {});
			if (serial === inspectSerial) {
				Object.assign(inspection, result, { offline: "", ready: true });
				emit("inspected");
			}
		}
		catch (error) {
			if (serial === inspectSerial) {
				Object.assign(inspection, { offline: error.message, ready: false });
				emit("inspected");
			}
		}
	}

	let inspectTimer = 0;
	function inspectSoon() {
		clearTimeout(inspectTimer);
		inspectTimer = setTimeout(inspect, 150);
	}

	function on(event, handler) {
		(listeners[event] = listeners[event] || []).push(handler);
	}

	function emit(event, ...args) {
		for (const handler of listeners[event] || []) {
			handler(...args);
		}
	}

	function changed(kind) {
		state.dirty[kind] = true;
		emit("change", kind);
		inspectSoon();
	}

	// The problems from the last check, or why there was no check.
	function errorsFor(kind) {
		if (inspection.offline) {
			return [inspection.offline];
		}
		return kind === "affixes" ? inspection.affixErrors : inspection.lootErrors;
	}

	// Writes the file through server.js. Without the server the copy is downloaded instead.
	async function save(kind) {
		clearTimeout(inspectTimer);
		await inspect();
		if (inspection.ready && errorsFor(kind).length > 0) {
			return { ok: false, message: "Not saved. Fix the problems listed first." };
		}
		const file = FILES[kind];
		const text = "var " + file.variable + " = " + JSON.stringify(state[kind], null, "\t") + ";\n";
		try {
			const response = await fetch(SERVER + file.url, {
				method: "POST",
				headers: { "Content-Type": "text/plain; charset=utf-8" },
				body: text
			});
			if (!response.ok) {
				throw new Error(await response.text());
			}
			state.dirty[kind] = false;
			emit("saved", kind);
			return { ok: true, message: "Saved " + file.name + ". A dev build picks it up on the next drop; a packed build needs Build + Reload." };
		}
		catch {
			const link = document.createElement("a");
			link.href = URL.createObjectURL(new Blob([text], { type: "text/javascript" }));
			link.download = file.name;
			link.click();
			setTimeout(() => URL.revokeObjectURL(link.href), 1500);
			return { ok: false, message: "Could not reach server.js, so " + file.name + " was downloaded instead." };
		}
	}

	function h(tag, props, ...children) {
		const node = document.createElement(tag);
		for (const [key, value] of Object.entries(props || {})) {
			if (value === undefined || value === null || value === false) {
				continue;
			}
			if (key === "class") {
				node.className = value;
			}
			else if (key === "text") {
				node.textContent = value;
			}
			else if (key === "on") {
				for (const [event, handler] of Object.entries(value)) {
					node.addEventListener(event, handler);
				}
			}
			else if (key === "value" || key === "checked" || key === "selected") {
				node[key] = value;
			}
			else {
				node.setAttribute(key, value === true ? "" : String(value));
			}
		}
		for (const child of children.flat()) {
			if (child === null || child === undefined || child === false) {
				continue;
			}
			node.append(child instanceof Node ? child : document.createTextNode(String(child)));
		}
		return node;
	}

	function number(value, onInput, options) {
		options = options || {};
		return h("input", {
			type: "number",
			value: value === null || value === undefined ? "" : String(value),
			min: options.min,
			max: options.max,
			step: options.step,
			placeholder: options.placeholder,
			title: options.title,
			class: options.class,
			on: {
				input: event => {
					const raw = event.target.value;
					if (raw === "" && options.allowEmpty) {
						onInput(null);
						return;
					}
					const parsed = options.float ? Number(raw) : Math.trunc(Number(raw));
					onInput(Number.isFinite(parsed) ? parsed : 0);
				}
			}
		});
	}

	function text(value, onInput, options) {
		options = options || {};
		return h("input", {
			type: "text",
			value: value || "",
			placeholder: options.placeholder,
			list: options.list,
			class: options.class,
			autocomplete: "off",
			spellcheck: "false",
			on: {
				input: event => onInput(event.target.value),
				change: options.onChange ? event => options.onChange(event.target.value) : undefined
			}
		});
	}

	function select(options, value, onChange) {
		const control = h("select", { on: { change: event => onChange(event.target.value) } });
		for (const option of options) {
			const entry = typeof option === "string" ? { value: option, label: option } : option;
			control.append(h("option", { value: entry.value, text: entry.label, selected: entry.value === value }));
		}
		control.value = value;
		return control;
	}

	function button(label, onClick, options) {
		options = options || {};
		return h("button", { type: "button", text: label, class: options.class, title: options.title, on: { click: onClick } });
	}

	function errors(list) {
		return h("ul", { class: "errors" }, list.map(message => h("li", { text: message })));
	}

	function datalist(id, values) {
		return h("datalist", { id }, [...new Set(values)].filter(Boolean).sort().map(value => h("option", { value })));
	}

	// A save button, a status note and a live list of problems for one file.
	function saveBar(kind) {
		const note = h("p", { class: "note" });
		const problems = h("div");
		const refresh = () => {
			const list = errorsFor(kind);
			problems.replaceChildren(errors(list));
			if (state.dirty[kind] && !note.dataset.sticky) {
				note.textContent = list.length > 0 ? "Unsaved changes, with problems." : "Unsaved changes.";
			}
		};
		on("change", changedKind => {
			if (changedKind === kind || kind === "loot") {
				delete note.dataset.sticky;
			}
		});
		on("inspected", refresh);
		const saveButton = button("Save " + FILES[kind].name, async () => {
			const result = await save(kind);
			note.textContent = result.message;
			note.dataset.sticky = "1";
			refresh();
		});
		refresh();
		return { node: h("div", { class: "toolbar" }, saveButton, note), problems };
	}

	global.addEventListener("beforeunload", event => {
		if (state.dirty.affixes || state.dirty.loot) {
			event.preventDefault();
			event.returnValue = "";
		}
	});

	global.RiftEditor = {
		RARITIES, state, meta, inspection, api, on, emit, changed, save, errorsFor, saveBar,
		normalizeRarity, h, number, text, select, button, errors, datalist
	};

	loadMeta();
	inspect();
})(globalThis);
