using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Utilities;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.Graphics.Capture;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using ReLogic.Graphics;
using System.Runtime;
using Microsoft.Xna.Framework.Input;
using Terraria.Graphics.Shaders;


namespace ScrollTooltip
{
	public class ScrollTooltip : Mod{
		[Label("Scrollable Tooltip")]
		public class MyConfig : ModConfig
		{
			public override ConfigScope Mode => ConfigScope.ClientSide;
			public static MyConfig get => ModContent.GetInstance<MyConfig>();

			[Header("Settings")]

			[Label("Box tooltip")]
			[Tooltip("Add boxes to scrollable tooltip")]
			[DefaultValue(true)]
			public bool DrawBox;

			[Label("Scroll Hint")]
			[Tooltip("Add a scroll hint")]
			[DefaultValue(true)]
			public bool ScrollHint;

			[Label("Max Line")]
			[Tooltip("The max scroll line")]
			[Range(2, 15)]
			[Increment(1)]
			[DefaultValue(5)]
			[DrawTicks]
			[Slider] 
			public int MaxLine;
		}		
		public override void UpdateUI(GameTime gameTime) {
			if (Main.HoverItem.IsAir) {
				Tooltips.scroll = 0;
				Tooltips.scrollable = false;
			}
		}
		public class Tooltips : GlobalItem {
			public static int scroll;
			public static bool scrollable = false;
			public override void ModifyTooltips(Item item,List<TooltipLine> tooltips) {
				int maxLine = MyConfig.get.MaxLine;
				List<TooltipLine> cachedLine = new List<TooltipLine>();
				scrollable = false;
				int firstIndex = -1;
				for (int i = 0; i < tooltips.Count; i++) {
					var tt = tooltips[i];
					if ((tt.mod == "Terraria" && tt.Name.Contains("Tooltip")) || (tt.mod != "Terraria" && !tt.isModifier && !tt.isModifierBad)) {
						cachedLine.Add(tt);
						if (firstIndex == -1){firstIndex = i;}
					}
				}
				int num25 = PlayerInput.ScrollWheelDelta / 120;
				if (num25 < 0) {scroll++;}
				if (num25 > 0) {scroll--;}
				if (scroll < 0) {scroll = 0;}
				if (scroll > cachedLine.Count-2) {scroll = cachedLine.Count-2;}
				if (cachedLine.Count > maxLine + 1) {
					if (firstIndex != -1 && MyConfig.get.ScrollHint) {
						tooltips.Insert(firstIndex - 1,new TooltipLine(mod, "ScrollHint", "Use mouse wheel to scroll"));
					}
					int lineRemove = cachedLine.Count - maxLine - 1;
					
					lineRemove -= scroll;

					if (scroll > 0) {
						lineRemove -= 1;
						for (int z = 0; z < scroll+1; z++){
							tooltips.Remove(cachedLine[z]);
						}
					}
					for (int b = cachedLine.Count - 1; b >= cachedLine.Count - lineRemove - 1 ; b--){
						if (b < cachedLine.Count) {
							tooltips.Remove(cachedLine[b]);
						}
					}
					scrollable = true;
				}

			}
			public static Vector2 Size(string longestText) {
				var snippets = ChatManager.ParseMessage(longestText, Color.White).ToArray();
				return ChatManager.GetStringSize(Main.fontMouseText, snippets, Vector2.One);
			}
			public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
				if (line.mod != "ScrollTooltip" && MyConfig.get.DrawBox && scrollable && ((line.mod == "Terraria" && line.Name.Contains("Tooltip")) || (line.mod != "Terraria" && !line.isModifier && !line.isModifierBad))) {
					Color color = Color.Blue;
					Vector2 messageSize = Size(line.text);
					int width = (int)messageSize.X + 3;
					if (200 > width) {width = 200;}
					Utils.DrawInvBG(Main.spriteBatch, new Rectangle(
						line.X - (35/10),
						line.Y - (42/10),
						width,
						(int)messageSize.Y),color*0.4f);
				}
				return base.PreDrawTooltipLine(item,line,ref yOffset);
			}
		}
	}
}