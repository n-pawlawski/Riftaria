using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Riftaria.Common.Skills;
using Riftaria.UI;

namespace Riftaria.Common.Players
{
	/// <summary>
	/// Character level, experience, and skill points, plus the nodes bought with
	/// them. Saved on the character, not the world.
	/// </summary>
	public class RiftLevelPlayer : ModPlayer
	{
		public int Level { get; private set; } = 1;

		public int Experience { get; private set; }

		public int SkillPoints { get; private set; }

		public int NodesUnlocked { get; private set; }

		public int XpToNext => 100 * Level;

		public bool CanUnlockNext => SkillPoints > 0 && NodesUnlocked < SkillTree.LifeLineLength;

		public void AddExperience(int amount)
		{
			if (amount <= 0) {
				return;
			}

			Experience += amount;
			if (Player.whoAmI == Main.myPlayer) {
				CombatText.NewText(Player.getRect(), new Color(140, 200, 255), $"+{amount} XP");
			}

			while (Experience >= XpToNext) {
				Experience -= XpToNext;
				Level++;
				SkillPoints++;
				OnLevelUp();
			}
		}

		public bool TryUnlockNext()
		{
			if (!CanUnlockNext) {
				return false;
			}

			SkillPoints--;
			NodesUnlocked++;
			return true;
		}

		private void OnLevelUp()
		{
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			CombatText.NewText(Player.getRect(), new Color(255, 215, 90), Language.GetTextValue("Mods.Riftaria.UI.LevelUp", Level), dramatic: true);
			Main.NewText(Language.GetTextValue("Mods.Riftaria.UI.LevelUp", Level), 255, 215, 90);
		}

		public override void PostUpdateEquips()
		{
			Player.statLifeMax2 += NodesUnlocked * SkillTree.LifePerNode;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (SkillTreeUISystem.ToggleKey?.JustPressed == true) {
				ModContent.GetInstance<SkillTreeUISystem>().Toggle();
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["level"] = Level;
			tag["experience"] = Experience;
			tag["skillPoints"] = SkillPoints;
			tag["nodesUnlocked"] = NodesUnlocked;
		}

		public override void LoadData(TagCompound tag)
		{
			Level = tag.ContainsKey("level") ? tag.GetInt("level") : 1;
			Experience = tag.GetInt("experience");
			SkillPoints = tag.GetInt("skillPoints");
			NodesUnlocked = System.Math.Min(tag.GetInt("nodesUnlocked"), SkillTree.LifeLineLength);
		}
	}
}
