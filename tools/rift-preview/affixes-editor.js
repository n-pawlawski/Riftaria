// The Affixes tab: every affix, stat, tag, rarity count and rare name in affixes.js.
(function () {
	"use strict";

	const E = RiftEditor;
	const h = E.h;
	const panel = document.querySelector('[data-panel="affixes"]');
	const view = { sub: "affixes", selected: 0, filter: { text: "", kind: "", tag: "", stat: "" } };
	const SUBS = [
		{ id: "affixes", label: "Affixes" },
		{ id: "stats", label: "Stats" },
		{ id: "tags", label: "Tags" },
		{ id: "rarities", label: "Rarities and names" }
	];

	const bar = E.saveBar("affixes");
	const subnav = h("div", { class: "toolbar subnav" });
	const body = h("div");
	panel.append(
		h("h2", { text: "Affixes" }),
		h("p", { class: "note", text: "An item holds at most one affix per group. Magic items roll up to 1 prefix and 1 suffix, Rare items up to 3 of each. An affix's chance is the summed weight of the tiers the item level unlocks. Tier 1 is the best tier." }),
		bar.node,
		bar.problems,
		subnav,
		body
	);

	function file() {
		return E.state.affixes;
	}

	function touch() {
		E.changed("affixes");
	}

	function render() {
		subnav.replaceChildren(...SUBS.map(sub => E.button(sub.label, () => {
			view.sub = sub.id;
			render();
		}, { class: view.sub === sub.id ? "active" : "" })));
		const views = { affixes: renderAffixes, stats: renderStats, tags: renderTags, rarities: renderRarities };
		body.replaceChildren(views[view.sub]());
	}

	// ---- Affixes ----------------------------------------------------------

	const listNode = h("div", { class: "list" });
	const countNote = h("p", { class: "note" });

	function matches(affix) {
		const f = view.filter;
		const needle = f.text.trim().toLowerCase();
		if (needle && !(affix.id.toLowerCase().includes(needle) || affix.name.toLowerCase().includes(needle))) {
			return false;
		}
		return (!f.kind || affix.kind === f.kind)
			&& (!f.tag || affix.tags.includes(f.tag))
			&& (!f.stat || affix.stat === f.stat);
	}

	// From the last server check, which may trail the newest keystroke slightly.
	function affixProblems(index) {
		return E.inspection.affixProblems?.[index] || [];
	}

	function findStat(id) {
		return file().stats.find(stat => stat.id === id) || null;
	}

	function renderList() {
		const affixes = file().affixes;
		const shown = [];
		affixes.forEach((affix, index) => {
			if (matches(affix)) {
				shown.push(index);
			}
		});
		countNote.textContent = shown.length + " of " + affixes.length + " affixes";
		listNode.replaceChildren(...shown.map(index => {
			const affix = affixes[index];
			const bad = affixProblems(index).length > 0;
			return h("div", {
				class: "list-row" + (index === view.selected ? " selected" : ""),
				on: { click: () => {
					view.selected = index;
					render();
				} }
			},
				h("span", {}, affix.id || "(no id)", " ", h("span", { class: "note", text: affix.name })),
				h("span", {},
					bad ? h("span", { class: "badge bad", text: "problem" }) : null, " ",
					h("span", { class: "badge " + affix.kind, text: affix.kind })),
				h("span", { class: "sub", text: affix.stat + " | " + affix.tags.join(", ") + " | " + affix.tiers.length + " tiers" })
			);
		}));
	}

	function renderAffixes() {
		const f = view.filter;
		const stats = file().stats.map(stat => stat.id);
		const filters = h("div", { class: "toolbar" },
			E.text(f.text, value => {
				f.text = value;
				renderList();
			}, { placeholder: "Search id or name" }),
			E.select([{ value: "", label: "Any kind" }, "prefix", "suffix"], f.kind, value => {
				f.kind = value;
				renderList();
			}),
			E.select([{ value: "", label: "Any tag" }, ...file().tags], f.tag, value => {
				f.tag = value;
				renderList();
			}),
			E.select([{ value: "", label: "Any stat" }, ...stats], f.stat, value => {
				f.stat = value;
				renderList();
			})
		);
		const add = E.button("Add affix", () => {
			file().affixes.push({
				id: uniqueId("NewAffix"),
				name: "",
				kind: f.kind || "prefix",
				group: "",
				stat: f.stat || stats[0] || "",
				tags: f.tag ? [f.tag] : [],
				tiers: [{ minLevel: 1, min: 1, max: 1, weight: 1000 }]
			});
			view.selected = file().affixes.length - 1;
			touch();
			render();
		});
		renderList();
		const left = h("div", { class: "card" }, filters, h("div", { class: "toolbar", style: "margin: 8px 0" }, add, countNote), listNode);
		return h("div", { class: "split" }, left, renderDetail());
	}

	function uniqueId(base) {
		const ids = new Set(file().affixes.map(affix => affix.id));
		let id = base;
		for (let i = 2; ids.has(id); i++) {
			id = base + i;
		}
		return id;
	}

	function renderDetail() {
		const affix = file().affixes[view.selected];
		if (!affix) {
			detailChecks = () => {};
			return h("div", { class: "card" }, h("p", { class: "note", text: "Pick an affix on the left, or add one." }));
		}

		const groups = file().affixes.map(entry => entry.group).filter(Boolean);
		const index = view.selected;
		const problems = h("div");
		const summary = h("p", { class: "note" });
		detailChecks = () => {
			problems.replaceChildren(E.errors(affixProblems(index)));
			summary.textContent = levelSummary(index);
		};
		const refresh = () => renderList();

		const statOptions = file().stats.map(stat => ({ value: stat.id, label: stat.id + "  (" + stat.label + ")" }));
		if (!file().stats.some(stat => stat.id === affix.stat)) {
			statOptions.unshift({ value: affix.stat, label: affix.stat + "  (unknown)" });
		}
		const statNote = h("span", { class: "note" });
		const describeStat = () => {
			const stat = findStat(affix.stat);
			statNote.textContent = stat ? stat.scope + ", " + stat.format : "";
		};
		describeStat();

		const tagChecks = h("div", { class: "checks" }, file().tags.map(tag => h("label", {},
			h("input", {
				type: "checkbox",
				checked: affix.tags.includes(tag),
				on: { change: event => {
					affix.tags = event.target.checked
						? file().tags.filter(t => t === tag || affix.tags.includes(t))
						: affix.tags.filter(t => t !== tag);
					touch();
					refresh();
				} }
			}),
			tag
		)));

		const fields = h("div", { class: "fields" },
			"Id", E.text(affix.id, value => {
				affix.id = value.trim();
				touch();
				refresh();
			}, { class: "wide" }),
			"Name", h("span", {}, E.text(affix.name, value => {
				affix.name = value;
				touch();
			}, { class: "wide", placeholder: affix.kind === "prefix" ? "Heavy" : "of Haste" }), " ", h("span", { class: "note", text: "Used in Magic item names" })),
			"Kind", E.select(["prefix", "suffix"], affix.kind, value => {
				affix.kind = value;
				touch();
				render();
			}),
			"Group", h("span", {}, E.text(affix.group, value => {
				affix.group = value.trim();
				touch();
				refresh();
			}, { class: "wide", list: "affix-groups", placeholder: affix.id }), " ", h("span", { class: "note", text: "Empty means the id" }), E.datalist("affix-groups", groups)),
			"Stat", h("span", {}, E.select(statOptions, affix.stat, value => {
				affix.stat = value;
				touch();
				describeStat();
				tierTable.replaceChildren(...tierRows(affix, refresh));
				refresh();
			}), " ", statNote),
			"Tags", tagChecks
		);

		const tierTable = h("tbody", {}, tierRows(affix, refresh));
		const tiers = h("table", {},
			h("thead", {}, h("tr", {}, ["Tier", "Min level", "Min", "Max", "Weight", "Rolls", ""].map(label => h("th", { text: label })))),
			tierTable
		);

		const actions = h("div", { class: "toolbar", style: "margin-top: 8px" },
			E.button("Add tier", () => {
				const last = affix.tiers[affix.tiers.length - 1];
				affix.tiers.push(last
					? { minLevel: last.minLevel + 1, min: last.max + 1, max: last.max + 1 + (last.max - last.min), weight: Math.max(1, Math.round(last.weight * 0.7)) }
					: { minLevel: 1, min: 1, max: 1, weight: 1000 });
				touch();
				render();
			}),
			E.button("Sort tiers by level", () => {
				affix.tiers.sort((a, b) => a.minLevel - b.minLevel);
				touch();
				render();
			}),
			h("span", { style: "flex: 1" }),
			E.button("Duplicate affix", () => {
				const copy = JSON.parse(JSON.stringify(affix));
				copy.id = uniqueId(affix.id + "Copy");
				file().affixes.splice(view.selected + 1, 0, copy);
				view.selected++;
				touch();
				render();
			}),
			E.button("Delete affix", () => {
				if (!confirm("Delete " + (affix.id || "this affix") + "?")) {
					return;
				}
				file().affixes.splice(view.selected, 1);
				view.selected = Math.min(view.selected, file().affixes.length - 1);
				touch();
				render();
			})
		);

		detailChecks();
		return h("div", { class: "card" }, h("h3", { text: affix.id || "(no id)" }), fields, h("h3", { text: "Tiers" }), tiers, actions, summary, problems);
	}

	function tierRows(affix, refresh) {
		return affix.tiers.map((tier, index) => {
			const rolls = h("td", { class: "note" });
			const label = () => {
				const stat = findStat(affix.stat);
				const text = stat && stat.label ? stat.label : affix.stat + " #";
				const range = tier.min === tier.max ? String(tier.min) : tier.min + " to " + tier.max;
				rolls.textContent = text.split("#").join(range);
			};
			label();
			const edit = key => E.number(tier[key], value => {
				tier[key] = value;
				touch();
				label();
				refresh();
			}, { min: key === "minLevel" ? 1 : undefined });
			return h("tr", {},
				h("td", { text: "T" + (affix.tiers.length - index) }),
				h("td", {}, edit("minLevel")),
				h("td", {}, edit("min")),
				h("td", {}, edit("max")),
				h("td", {}, edit("weight")),
				rolls,
				h("td", {}, E.button("Remove", () => {
					affix.tiers.splice(index, 1);
					touch();
					render();
				}))
			);
		});
	}

	function levelSummary(index) {
		const weights = E.inspection.affixWeights?.[index] || [];
		if (weights.length === 0) {
			return "";
		}
		return "Roll weight by item level: " + weights.map(entry => "level " + entry.level + " = " + entry.weight).join(", ") + ".";
	}

	// Set while an affix detail is showing; refreshes its checks after each server check.
	let detailChecks = () => {};

	// ---- Stats ------------------------------------------------------------

	function renderStats() {
		const stats = file().stats;
		const used = id => file().affixes.filter(affix => affix.stat === id).length;
		const rows = stats.map((stat, index) => {
			const options = E.meta.stats.filter(id => id === stat.id || !stats.some(other => other.id === id));
			if (!options.includes(stat.id)) {
				options.unshift(stat.id);
			}
			return h("tr", {},
				h("td", {}, E.select(options, stat.id, value => {
					for (const affix of file().affixes) {
						if (affix.stat === stat.id) {
							affix.stat = value;
						}
					}
					stat.id = value;
					touch();
					render();
				})),
				h("td", {}, E.text(stat.label, value => {
					stat.label = value;
					touch();
				}, { class: "wide", placeholder: "#% increased damage" })),
				h("td", {}, E.select(["flat", "percent"], stat.format, value => {
					stat.format = value;
					touch();
				})),
				h("td", {}, E.select(["local", "global"], stat.scope, value => {
					stat.scope = value;
					touch();
				})),
				h("td", { class: "note", text: used(stat.id) + " affixes" }),
				h("td", {}, E.button("Remove", () => {
					if (used(stat.id) > 0 && !confirm(stat.id + " is used by " + used(stat.id) + " affixes. Remove it anyway?")) {
						return;
					}
					stats.splice(index, 1);
					touch();
					render();
				}))
			);
		});
		const free = E.meta.stats.filter(id => !stats.some(stat => stat.id === id));
		return h("div", { class: "card" },
			h("p", { class: "note", text: "Stat ids come from StatId.cs in the game, so new ids need code first. The label puts the rolled value where the # is. Local stats only change the item they are on; global stats add to the player while the item is equipped." }),
			h("table", {},
				h("thead", {}, h("tr", {}, ["Stat", "Label", "Format", "Scope", "Used by", ""].map(label => h("th", { text: label })))),
				h("tbody", {}, rows)
			),
			h("div", { class: "toolbar", style: "margin-top: 8px" },
				free.length > 0
					? E.button("Add stat", () => {
						stats.push({ id: free[0], label: free[0] + " #", format: "flat", scope: "global" });
						touch();
						render();
					})
					: h("span", { class: "note", text: E.meta.ready ? "Every stat the game knows is listed." : "Waiting for server.js to list the game's stats." }))
		);
	}

	// ---- Tags -------------------------------------------------------------

	function renderTags() {
		const tags = file().tags;
		const loot = E.state.loot;
		const usedBy = tag => file().affixes.filter(affix => affix.tags.includes(tag)).length;
		const basesUsing = tag => loot.bases.filter(entry => entry.tag === tag).length;
		const rows = tags.map((tag, index) => h("tr", {},
			h("td", {}, E.text(tag, () => {}, { onChange: value => {
				value = value.trim();
				if (!value || value === tag) {
					render();
					return;
				}
				if (tags.includes(value)) {
					alert("Tag " + value + " already exists.");
					render();
					return;
				}
				tags[index] = value;
				for (const affix of file().affixes) {
					affix.tags = affix.tags.map(t => t === tag ? value : t);
				}
				let lootChanged = false;
				for (const entry of loot.bases) {
					if (entry.tag === tag) {
						entry.tag = value;
						lootChanged = true;
					}
				}
				touch();
				if (lootChanged) {
					E.changed("loot");
				}
				render();
			} })),
			h("td", { class: "note", text: usedBy(tag) + " affixes, " + basesUsing(tag) + " bases" }),
			h("td", {}, E.button("Remove", () => {
				const warn = basesUsing(tag) > 0 ? " " + basesUsing(tag) + " bases in loot-pools.js still use it." : "";
				if (!confirm("Remove tag " + tag + " from every affix?" + warn)) {
					return;
				}
				tags.splice(index, 1);
				for (const affix of file().affixes) {
					affix.tags = affix.tags.filter(t => t !== tag);
				}
				touch();
				E.changed("loot");
				render();
			}))
		));
		const input = E.text("", () => {}, { placeholder: "new tag" });
		return h("div", { class: "card" },
			h("p", { class: "note", text: "A base in loot-pools.js has one tag, and only affixes carrying that tag can roll on it. Renaming a tag here renames it on every affix and base." }),
			h("table", {}, h("tbody", {}, rows)),
			h("div", { class: "toolbar", style: "margin-top: 8px" }, input, E.button("Add tag", () => {
				const value = input.value.trim();
				if (!value || tags.includes(value)) {
					return;
				}
				tags.push(value);
				touch();
				render();
			}))
		);
	}

	// ---- Rarities and names -----------------------------------------------

	function renderRarities() {
		const rarities = file().rarities;
		const names = file().rareNames;
		const countTable = (key, label, cap) => {
			const counts = rarities[key];
			const body = h("tbody");
			const fill = () => {
				const total = counts.reduce((sum, count) => sum + Math.max(0, count.weight), 0);
				body.replaceChildren(...counts.map((count, index) => {
					const share = h("td", { class: "note", text: total > 0 ? (Math.max(0, count.weight) / total * 100).toFixed(1) + "%" : "" });
					return h("tr", {},
						h("td", {}, E.number(count.count, value => {
							count.count = value;
							touch();
						}, { min: 1, max: cap })),
						h("td", {}, E.number(count.weight, value => {
							count.weight = value;
							touch();
							fill();
						}, { min: 0 })),
						share,
						h("td", {}, E.button("Remove", () => {
							counts.splice(index, 1);
							touch();
							fill();
						}))
					);
				}));
			};
			fill();
			return h("div", { class: "card" },
				h("h3", { text: label + " (1 to " + cap + " affixes)" }),
				h("table", {}, h("thead", {}, h("tr", {}, ["Affixes", "Weight", "Chance", ""].map(t => h("th", { text: t })))), body),
				E.button("Add count", () => {
					counts.push({ count: cap, weight: 10 });
					touch();
					fill();
				})
			);
		};

		const words = (key, label) => h("div", {},
			h("h3", { text: label }),
			h("textarea", {
				value: names[key].join("\n"),
				on: { input: event => {
					names[key] = event.target.value.split(/\r?\n/).map(word => word.trim()).filter(Boolean);
					touch();
				} }
			})
		);

		const samples = h("p", { class: "note" });
		const sample = async () => {
			try {
				samples.textContent = (await E.api("names", { count: 8 })).join(", ");
			}
			catch (error) {
				samples.textContent = error.message;
			}
		};

		return h("div", {},
			h("p", { class: "note", text: "How many affixes a roll aims for. A roll stops early when nothing else fits the base." }),
			h("div", { class: "grid-cards" },
				countTable("magic", "Magic", E.meta.caps.magic * 2),
				countTable("rare", "Rare", E.meta.caps.rare * 2)
			),
			h("div", { class: "card", style: "margin-top: 12px" },
				h("p", { class: "note", text: "A Rare item's name is one word from each list. One word per line." }),
				h("div", { class: "grid-cards" }, words("first", "First word"), words("second", "Second word")),
				h("div", { class: "toolbar", style: "margin-top: 8px" }, E.button("Sample names", sample), samples)
			)
		);
	}

	E.on("show", name => {
		if (name === "affixes") {
			render();
		}
	});
	E.on("saved", kind => {
		if (kind === "affixes" && !panel.hidden) {
			renderList();
		}
	});
	E.on("inspected", () => {
		if (!panel.hidden && view.sub === "affixes") {
			renderList();
			detailChecks();
		}
	});
	E.on("meta", () => {
		if (!panel.hidden && view.sub !== "affixes") {
			render();
		}
	});
	render();
})();
