// The Loot pools tab: bases, per-tier general and boss pools, and boss overrides in loot-pools.js.
(function () {
	"use strict";

	const E = RiftEditor;
	const h = E.h;
	const panel = document.querySelector('[data-panel="loot"]');
	const KNOWN_BOSSES = ["RiftZombie", "RiftEye"];
	const view = { tag: "" };

	const bar = E.saveBar("loot");
	const body = h("div", { style: "display: flex; flex-direction: column; gap: 12px" });
	panel.append(
		h("h2", { text: "Loot pools" }),
		h("p", { class: "note", text: "Each rift draws its own bases from the ones that fit its tier: one set for enemy kills (and later chests), one for the boss bag. Bases are vanilla ItemID names." }),
		bar.node,
		bar.problems,
		body
	);

	function loot() {
		return E.state.loot;
	}

	function touch() {
		E.changed("loot");
	}

	function render() {
		body.replaceChildren(renderBases(), renderTiers(), renderBosses());
		showFits();
	}

	function rarityInputs(weights, onChange) {
		const share = h("span", { class: "note" });
		const fill = () => {
			const total = E.RARITIES.reduce((sum, r) => sum + Math.max(0, weights[r]), 0);
			share.textContent = total > 0
				? E.RARITIES.map(r => (Math.max(0, weights[r]) / total * 100).toFixed(0) + "%").join(" / ")
				: "no weight, rolls Normal";
		};
		fill();
		return h("span", { class: "toolbar" },
			...E.RARITIES.map(r => h("label", { class: "rarity-" + r }, r[0].toUpperCase() + r.slice(1),
				E.number(weights[r], value => {
					weights[r] = value;
					fill();
					onChange();
				}, { min: 0 }))),
			share
		);
	}

	// ---- Bases ------------------------------------------------------------

	function renderBases() {
		const tags = E.state.affixes.tags;
		const bases = loot().bases;
		const rows = [];
		bases.forEach((entry, index) => {
			if (view.tag && entry.tag !== view.tag) {
				return;
			}
			const tagOptions = tags.includes(entry.tag) ? tags : [entry.tag, ...tags];
			rows.push(h("tr", {},
				h("td", {}, E.text(entry.item, value => {
					entry.item = value.trim();
					touch();
				}, { class: "wide", placeholder: "WoodenBow" })),
				h("td", {}, E.select(tagOptions, entry.tag, value => {
					entry.tag = value;
					touch();
				})),
				h("td", {}, E.number(entry.minTier, value => {
					entry.minTier = value;
					touch();
				}, { min: 1 })),
				h("td", {}, E.number(entry.maxTier, value => {
					entry.maxTier = value;
					touch();
				}, { min: 0, title: "0 means no upper limit" })),
				h("td", {}, E.number(entry.weight, value => {
					entry.weight = value;
					touch();
				}, { min: 0 })),
				h("td", {}, E.button("Remove", () => {
					bases.splice(index, 1);
					touch();
					render();
				}))
			));
		});

		return h("div", { class: "card" },
			h("h3", { text: "Bases" }),
			h("div", { class: "toolbar", style: "margin-bottom: 6px" },
				E.select([{ value: "", label: "Every tag" }, ...tags], view.tag, value => {
					view.tag = value;
					render();
				}),
				E.button("Add base", () => {
					bases.push({ item: "", tag: view.tag || tags[0] || "", minTier: 1, maxTier: 0, weight: 100 });
					touch();
					render();
				}),
				h("span", { class: "note", text: bases.length + " bases. Max tier 0 means no upper limit." })
			),
			h("table", {},
				h("thead", {}, h("tr", {}, ["Item", "Tag", "Min tier", "Max tier", "Weight", ""].map(t => h("th", { text: t })))),
				h("tbody", {}, rows)
			)
		);
	}

	// ---- Tiers ------------------------------------------------------------

	// Refreshed after each server check, which knows which bases fit each tier.
	let fitNotes = [];

	function showFits() {
		fitNotes.forEach((note, index) => {
			const entry = E.inspection.tiers?.[index];
			note.textContent = entry ? entry.bases.length + " bases fit this tier" : "";
		});
	}

	function renderTiers() {
		const tiers = loot().tiers;
		fitNotes = [];
		const cards = tiers.map((tier, index) => {
			const fits = h("span", { class: "note" });
			fitNotes.push(fits);
			const general = tier.general;
			const boss = tier.boss;
			return h("div", { class: "card" },
				h("div", { class: "toolbar" },
					h("h3", { text: "Tier", style: "margin: 0" }),
					E.number(tier.tier, value => {
						tier.tier = value;
						touch();
					}, { min: 1 }),
					fits,
					h("span", { style: "flex: 1" }),
					E.button("Remove tier", () => {
						if (!confirm("Remove tier " + tier.tier + "?")) {
							return;
						}
						tiers.splice(index, 1);
						touch();
						render();
					})
				),
				h("h3", { text: "Enemy kills", style: "margin-top: 10px" }),
				h("div", { class: "fields" },
					"Drop chance %", E.number(Math.round(general.dropChance * 10000) / 100, value => {
						general.dropChance = value / 100;
						touch();
					}, { min: 0, max: 100, step: 0.1, float: true }),
					"Bases per rift", h("span", {}, E.number(general.baseCount, value => {
						general.baseCount = value;
						touch();
					}, { min: 0 }), " ", h("span", { class: "note", text: "0 keeps every base that fits" })),
					"Rarity", rarityInputs(general.rarity, touch)
				),
				h("h3", { text: "Boss bag" }),
				h("div", { class: "fields" },
					"Items per bag", E.number(boss.items, value => {
						boss.items = value;
						touch();
					}, { min: 0 }),
					"Bases per rift", E.number(boss.baseCount, value => {
						boss.baseCount = value;
						touch();
					}, { min: 0 }),
					"Rarity", rarityInputs(boss.rarity, touch)
				)
			);
		});

		return h("div", {},
			h("div", { class: "toolbar", style: "margin-bottom: 8px" },
				h("h3", { text: "Tiers", style: "margin: 0" }),
				E.button("Add tier", () => {
					const last = tiers[tiers.length - 1];
					const copy = last ? JSON.parse(JSON.stringify(last)) : {
						tier: 0,
						general: { dropChance: 0.05, baseCount: 0, rarity: { normal: 60, magic: 30, rare: 10 } },
						boss: { items: 1, baseCount: 0, rarity: { normal: 0, magic: 60, rare: 40 } }
					};
					copy.tier = (last ? last.tier : 0) + 1;
					tiers.push(copy);
					touch();
					render();
				}),
				h("span", { class: "note", text: "A rift above the last tier uses the highest tier below it." })
			),
			h("div", { class: "grid-cards" }, cards)
		);
	}

	E.on("inspected", showFits);

	// ---- Boss overrides ---------------------------------------------------

	function renderBosses() {
		const bosses = loot().bosses;
		const rows = bosses.map((entry, index) => {
			const rarityCell = h("td");
			const fillRarity = () => {
				rarityCell.replaceChildren(entry.rarity
					? rarityInputs(entry.rarity, touch)
					: h("span", { class: "note", text: "tier's rarity" }));
			};
			fillRarity();
			return h("tr", {},
				h("td", {}, E.text(entry.boss, value => {
					entry.boss = value.trim();
					touch();
				}, { list: "known-bosses", placeholder: "RiftZombie" })),
				h("td", {}, E.number(entry.tier, value => {
					entry.tier = value;
					touch();
				}, { min: 0, title: "0 applies to every tier" })),
				h("td", {}, E.number(entry.items, value => {
					entry.items = value;
					touch();
				}, { min: 0, allowEmpty: true, placeholder: "tier's" })),
				h("td", {}, h("label", {}, h("input", {
					type: "checkbox",
					checked: !!entry.rarity,
					on: { change: event => {
						entry.rarity = event.target.checked ? { normal: 0, magic: 50, rare: 50 } : null;
						touch();
						fillRarity();
					} }
				}), "own rarity")),
				rarityCell,
				h("td", {}, E.button("Remove", () => {
					bosses.splice(index, 1);
					touch();
					render();
				}))
			);
		});

		return h("div", { class: "card" },
			h("h3", { text: "Boss overrides" }),
			h("p", { class: "note", text: "Changes the boss bag for one boss. Tier 0 applies to every tier, and an override for the exact tier wins. Leave items empty to keep the tier's count." }),
			E.datalist("known-bosses", [...KNOWN_BOSSES, ...bosses.map(entry => entry.boss)]),
			h("table", {},
				h("thead", {}, h("tr", {}, ["Boss", "Tier", "Items", "", "Rarity", ""].map(t => h("th", { text: t })))),
				h("tbody", {}, rows)
			),
			E.button("Add override", () => {
				bosses.push({ boss: KNOWN_BOSSES[0], tier: 0, items: null, rarity: null });
				touch();
				render();
			})
		);
	}

	E.on("show", name => {
		if (name === "loot") {
			render();
		}
	});
	render();
})();
