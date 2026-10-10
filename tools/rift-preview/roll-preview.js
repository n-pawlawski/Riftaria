// The Roll preview tab: rolls items from the current, unsaved edits with the game's own roller, through server.js.
(function () {
	"use strict";

	const E = RiftEditor;
	const h = E.h;
	const panel = document.querySelector('[data-panel="roll"]');
	const MAX_TRIALS = 1000000;
	const view = { tier: 1, source: "general", boss: "RiftZombie", base: -1, rarity: "pool", samples: 12, trials: 10000 };

	const controls = h("div", { class: "toolbar" });
	const planNote = h("p", { class: "note" });
	const cards = h("div", { class: "grid-cards" });
	const statsNode = h("div");
	const checkNote = h("span", { class: "note" });
	const checkNode = h("div");
	const checkButton = E.button("Rebuild and check", runCheck);
	panel.append(
		h("h2", { text: "Roll preview" }),
		h("p", { class: "note", text: "Rolls from what is in the other tabs right now, saved or not, using the mod's own C# roller. Pool sources draw a fresh rift plan each time, the way a new rift would." }),
		h("div", { class: "card", style: "margin-bottom: 12px" },
			h("h3", { text: "Full check" }),
			h("p", { class: "note", text: "Rebuilds the roller from the mod's C# and runs every DropCheck test on the current edits: data problems, base draws, rarity odds, drop chance, boss bags, and every rule on about 170,000 rolled items. C# changes still need Build + Reload in tModLoader to reach the game." }),
			h("div", { class: "toolbar" }, checkButton, checkNote),
			checkNode
		),
		controls,
		planNote,
		cards,
		h("h3", { text: "Many rolls", style: "margin-top: 8px" }),
		statsNode
	);

	async function runCheck() {
		checkButton.disabled = true;
		checkNote.textContent = "Rebuilding and checking...";
		checkNote.className = "note";
		checkNode.replaceChildren();
		const started = performance.now();
		try {
			const report = await E.api("check", { rebuild: true });
			const seconds = ((performance.now() - started) / 1000).toFixed(1);
			checkNote.textContent = (report.ok ? "Passed" : report.failures.length + " failed") + " in " + seconds + "s, on unsaved edits.";
			checkNote.className = report.ok ? "note check-ok" : "note check-bad";
			checkNode.replaceChildren(...[
				report.ok ? null : E.errors(report.failures),
				report.lines.length > 0 ? h("pre", { class: "check-log", text: report.lines.join("\n") }) : null
			].filter(Boolean));
		}
		catch (error) {
			checkNote.textContent = "";
			checkNote.className = "note";
			checkNode.replaceChildren(h("pre", { class: "check-log errors", text: error.message }));
		}
		finally {
			checkButton.disabled = false;
		}
	}

	function loot() {
		return E.state.loot;
	}

	function tiers() {
		return loot().tiers.map(tier => tier.tier).sort((a, b) => a - b);
	}

	function bosses() {
		return [...new Set(["RiftZombie", "RiftEye", ...loot().bosses.map(entry => entry.boss).filter(Boolean)])];
	}

	// Indices into the bases list, from the last server check.
	function fitting() {
		const index = loot().tiers.findIndex(tier => tier.tier === view.tier);
		return E.inspection.tiers?.[index]?.bases || [];
	}

	function renderControls() {
		const tierList = tiers();
		if (!tierList.includes(view.tier)) {
			view.tier = tierList[0] || 1;
		}
		const fits = fitting();
		if (!fits.includes(view.base)) {
			view.base = fits.length > 0 ? fits[0] : -1;
		}
		if (view.source === "base" && view.rarity === "pool") {
			view.rarity = "magic";
		}

		const rarityOptions = [
			...(view.source === "base" ? [] : [{ value: "pool", label: "Pool's rarity" }]),
			{ value: "normal", label: "Normal" },
			{ value: "magic", label: "Magic" },
			{ value: "rare", label: "Rare" }
		];
		const baseOptions = fits.map(index => {
			const entry = loot().bases[index];
			return { value: String(index), label: entry ? entry.item + " (" + entry.tag + ")" : "?" };
		});

		controls.replaceChildren(...[
			h("label", {}, "Tier", E.select(tierList.map(t => ({ value: String(t), label: String(t) })), String(view.tier), value => {
				view.tier = Number(value);
				renderControls();
			})),
			h("label", {}, "From", E.select([
				{ value: "general", label: "Enemy kill" },
				{ value: "boss", label: "Boss bag" },
				{ value: "base", label: "One base" }
			], view.source, value => {
				view.source = value;
				renderControls();
			})),
			view.source === "boss" ? h("label", {}, "Boss", E.select(bosses(), view.boss, value => {
				view.boss = value;
			})) : null,
			view.source === "base" ? h("label", {}, "Base", E.select(baseOptions, String(view.base), value => {
				view.base = Number(value);
			})) : null,
			h("label", {}, "Rarity", E.select(rarityOptions, view.rarity, value => {
				view.rarity = value;
			})),
			h("label", {}, "Samples", E.number(view.samples, value => {
				view.samples = Math.max(1, Math.min(200, value));
			}, { min: 1, max: 200 })),
			E.button("Roll samples", rollSamples),
			h("label", {}, "Rolls", E.number(view.trials, value => {
				view.trials = Math.max(1, Math.min(MAX_TRIALS, value));
			}, { min: 1, max: MAX_TRIALS })),
			E.button("Run", runStats)
		].filter(Boolean));
	}

	function request() {
		return { tier: view.tier, source: view.source, boss: view.boss, baseIndex: view.base, rarity: view.rarity };
	}

	async function rollSamples() {
		planNote.textContent = "Rolling...";
		try {
			const result = await E.api("samples", { ...request(), count: view.samples });
			planNote.textContent = result.note;
			cards.replaceChildren(...result.items.map(itemCard));
		}
		catch (error) {
			planNote.textContent = error.message;
			cards.replaceChildren();
		}
	}

	function itemCard(item) {
		return h("div", { class: "item-card" },
			h("div", { class: "name rarity-" + item.rarity, text: item.name }),
			h("div", { class: "base", text: (item.rarity === "rare" ? item.item + ", " : "") + item.tag + ", item level " + item.level + ", " + item.rarity }),
			item.affixes.map(affix => h("div", { class: "affix" },
				h("span", { class: "tier", text: "T" + affix.tier + " " + (affix.prefix ? "P" : "S") }),
				affix.text
			))
		);
	}

	async function runStats() {
		statsNode.replaceChildren(h("p", { class: "note", text: "Rolling " + view.trials.toLocaleString() + " items..." }));
		let stats;
		try {
			stats = await E.api("stats", { ...request(), trials: view.trials });
		}
		catch (error) {
			statsNode.replaceChildren(h("p", { class: "note", text: error.message }));
			return;
		}
		if (stats.rolled === 0) {
			statsNode.replaceChildren(h("p", { class: "note", text: "The pool has no bases for this tier." }));
			return;
		}

		const pct = (n, d) => d > 0 ? (n / d * 100).toFixed(1) + "%" : "-";
		const avg = (n, d) => d > 0 ? (n / d).toFixed(2) : "-";
		const rarityTable = h("table", {},
			h("thead", {}, h("tr", {}, ["Rarity", "Share", "Avg prefixes", "Avg suffixes"].map(t => h("th", { text: t })))),
			h("tbody", {}, stats.rarity.map(r => h("tr", {},
				h("td", { class: "rarity-" + r.rarity, text: r.rarity }),
				h("td", { text: pct(r.count, stats.rolled) }),
				h("td", { text: r.rarity === "normal" ? "0" : avg(r.prefixes, r.count) }),
				h("td", { text: r.rarity === "normal" ? "0" : avg(r.suffixes, r.count) })
			)))
		);

		const maxShare = Math.max(1, ...stats.affixes.map(entry => entry.count));
		const affixRows = stats.affixes.map(entry => h("tr", {},
			h("td", { text: entry.id }),
			h("td", { text: entry.prefix ? "prefix" : "suffix" }),
			h("td", {}, h("span", { class: "bar", style: "width: " + Math.round(entry.count / maxShare * 120) + "px" }), " ", pct(entry.count, stats.affixed)),
			h("td", { class: "note", text: entry.tiers.map(t => "T" + t.tier + " " + pct(t.count, entry.count)).join(", ") }),
			h("td", { text: (entry.total / entry.count).toFixed(1) })
		));

		const baseRows = stats.bases.map(entry => h("tr", {}, h("td", { text: entry.item }), h("td", { text: pct(entry.count, stats.rolled) })));

		statsNode.replaceChildren(...[
			h("p", { class: "note", text: stats.rolled.toLocaleString() + " items at tier " + view.tier + "." + (stats.rareShort > 0 ? " " + stats.rareShort + " Rare items ran out of affixes that fit and ended below 3." : "") }),
			h("div", { class: "grid-cards" },
				h("div", { class: "card" }, h("h3", { text: "Rarity" }), rarityTable),
				view.source === "base" ? null : h("div", { class: "card" }, h("h3", { text: "Bases" }), h("table", {}, h("tbody", {}, baseRows)))
			),
			h("div", { class: "card", style: "margin-top: 12px" },
				h("h3", { text: "Affixes on Magic and Rare items" }),
				h("table", {},
					h("thead", {}, h("tr", {}, ["Affix", "Kind", "On items", "Tiers", "Avg value"].map(t => h("th", { text: t })))),
					h("tbody", {}, affixRows)
				)
			)
		].filter(Boolean));
	}

	E.on("show", name => {
		if (name === "roll") {
			renderControls();
		}
	});
	E.on("inspected", () => {
		if (!panel.hidden) {
			renderControls();
		}
	});
	renderControls();
})();
