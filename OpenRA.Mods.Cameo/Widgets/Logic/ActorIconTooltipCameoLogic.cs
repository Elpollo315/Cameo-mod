#region Copyright & License Information
/*
 * Copyright 2007-2022 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Mods.AS.Widgets;
using OpenRA.Mods.CA.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Cameo.Widgets.Logic
{
	public class ActorIconTooltipCameoLogic : ChromeLogic
	{
		[ObjectCreator.UseCtor]
		public ActorIconTooltipCameoLogic(Widget widget, TooltipContainerWidget tooltipContainer, Func<BasicUnit> getTooltipUnit)
		{
			widget.IsVisible = () => getTooltipUnit() != null;
			var nameLabel = widget.Get<LabelWidget>("NAME");
			var extrasLabel = widget.Get<LabelWidget>("EXTRAS");
			var descLabel = widget.Get<LabelWidget>("DESC");

			var font = Game.Renderer.Fonts[nameLabel.Font];
			var extrasFont = Game.Renderer.Fonts[extrasLabel.Font];
			var descFont = Game.Renderer.Fonts[descLabel.Font];

			BasicUnit lastUnit = null;
			var descLabelPadding = descLabel.Bounds.Height;
			var descLabelY = descLabel.Bounds.Y;

			tooltipContainer.BeforeRender = () =>
			{
				var unit = getTooltipUnit();

				if (unit == null || unit == lastUnit)
					return;

				var world = unit.Actor?.World;
				var stance = world?.RenderPlayer == null ? PlayerRelationship.None : unit.Actor?.Owner.RelationshipWith(world.RenderPlayer);
				var tooltip = unit.Tooltips?.FirstEnabledTraitOrDefault();
				var name = tooltip?.TooltipInfo.TooltipForPlayerStance(stance.Value) ?? unit.ActorInfo.TraitInfos<TooltipInfo>().FirstOrDefault().Name;
				name ??= unit.Actor?.Info.Name ?? unit.ActorInfo.Name;
				var buildable = unit.BuildableInfo;
				var tooltipDescs = unit.TooltipDescriptions?.Where(td => td.IsTooltipVisible(world.RenderPlayer ?? world.LocalPlayer));

				nameLabel.Text = name;

				var nameSize = font.Measure(name);

				var extras = unit.ActorInfo.TraitInfos<TooltipExtrasInfo>();

				extrasLabel.Text = String.Join("\n", extras.Select(extra => TranslationProvider.GetString(extra.Description)));
				var extraSize = new int2(0, 0);

				if (extrasLabel.Text != "")
				{
					extraSize = extrasFont.Measure(extrasLabel.Text);
					extrasLabel.Visible = true;
					descLabel.Bounds.Y = descLabelY + extraSize.Y;
				}

				var descSize = new int2(0, 0);
				if (tooltipDescs != null && tooltipDescs.Any())
				{
					var descText = "";
					foreach (var tooltipDesc in tooltipDescs)
					{
						if (!string.IsNullOrEmpty(descText))
							descText += "\n";

						descText += TranslationProvider.GetString(tooltipDesc.TooltipText);
					}

					descLabel.Text = descText;
					descSize = descFont.Measure(descLabel.Text);
					descLabel.Bounds.Width = descSize.X;
					descLabel.Bounds.Height = descSize.Y + descLabelPadding;
				}
				else if (buildable != null && !string.IsNullOrEmpty(buildable.Description))
				{
					descLabel.Text = TranslationProvider.GetString(buildable.Description);
					descSize = descFont.Measure(descLabel.Text);
					descLabel.Bounds.Width = descSize.X;
					descLabel.Bounds.Height = descSize.Y + descLabelPadding;
				}
				else
				{
					descLabel.Bounds.Height = 0;
				}

				var leftWidth = Math.Max(nameSize.X, descSize.X);

				widget.Bounds.Width = leftWidth + 2 * nameLabel.Bounds.X;

				// Set the bottom margin to match the left margin
				var leftHeight = descLabel.Bounds.Bottom + descLabel.Bounds.X;

				widget.Bounds.Height = leftHeight;

				lastUnit = unit;
			};
		}
	}
}