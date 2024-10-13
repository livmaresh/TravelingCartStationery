using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using System.Security;
using StardewValley.Objects;
using StardewValley.Extensions;
using StardewValley.Quests;
using StardewValley.Buildings;
using Microsoft.Xna.Framework.Input;
using StardewValley.ItemTypeDefinitions;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Locations;
using xTile;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Content;
using System.Net.Http.Json;

namespace TravellingCartStationery
{
    internal sealed class ModEntry : Mod
    {

        private string letter = "";
        private string title = "";
        private bool mailAdded = false;
        private bool firstPass = true;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Display.MenuChanged += this.MenuChanged;
        }
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            int[] travelingCartDays = { 5, 7, 12, 14, 19, 21, 26, 28 };
            int[] festivalDays = { 15, 16, 17 };
            if (travelingCartDays.Contains(StardewValley.Game1.dayOfMonth))
            {
                mailAdded = true;
                title = "TCS " + getStardewDate();
                StardewValley.Game1.addMail(title.Replace(" ", ""));
                StardewValley.Utility.TryOpenShopMenu("Traveler", "AnyOrNone", false);
            }
            else if (StardewValley.Game1.CurrentSeasonDisplayName == "winter" && festivalDays.Contains(StardewValley.Game1.dayOfMonth))
            {
                mailAdded = true;
                title = "NM " + getStardewDate();
                StardewValley.Game1.addMail(title.Replace(" ", ""));
                StardewValley.Utility.TryOpenShopMenu("Traveler", "AnyOrNone", false);
            }
            else if (StardewValley.Game1.CurrentSeasonDisplayName == "spring" && festivalDays.Contains(StardewValley.Game1.dayOfMonth))
            {
                mailAdded = true;
                title = "DF " + getStardewDate();
                StardewValley.Game1.addMail(title.Replace(" ", ""));
                StardewValley.Utility.TryOpenShopMenu("Traveler", "AnyOrNone", false);
            }
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();
                    if (letter != "")
                    {
                        editor.Data[title.Replace(" ", "")] = letter;
                        letter = "";
                    }
                });
            }
        }

        private void MenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu != null)
            {
                var menuType = e.NewMenu.ToString() ?? "???";
                if (menuType == "StardewValley.Menus.ShopMenu")
                {
                    ShopMenu shopMenu = e.NewMenu as ShopMenu;
                    List<string> items = new List<string>();
                    if (shopMenu != null && shopMenu.ShopId == "Traveler" && mailAdded)
                    {
                        foreach (var item in shopMenu.forSale)
                        {
                            items.Add(item.Name.ToString());
                        }
                        letter = addTravelingCartStationery(items);
                        if (!firstPass)
                        {
                            Helper.GameContent.InvalidateCache("Data/mail");
                        }
                        firstPass = false;
                        DataLoader.Mail(Game1.content);
                        e.NewMenu.exitThisMenuNoSound();
                    }
                }
            }
        }

        private string addTravelingCartStationery(List<string> items)
        {
            var result = "";
            if (title.Contains("TCS")) result = "The Traveling Cart options for " + getStardewDate() + " are:^";
            else if (title.Contains("NM")) result = "The Night Market TC options for " + getStardewDate() + " are:^";
            else result = "The Desert Festival TC options for " + getStardewDate() + " are:^";

            var ii = 0;
            while (ii < items.Count)
            {
                if (ii + 1 == items.Count) result += items[ii] + " [letterbg 2] [textcolor white] [#]" + title;
                else result += items[ii] + "^";
                ii++;
            }

            mailAdded = false;
            return result;
        }

        private string getStardewDate()
        {
            return customToUpper(StardewValley.Game1.CurrentSeasonDisplayName) + " " + StardewValley.Game1.dayOfMonth + " Year " + StardewValley.Game1.year;
        }

        private string customToUpper(string subject)
        {
            if (subject == null) return "";
            if (subject.Length > 1) return char.ToUpper(subject[0]) + subject.Substring(1);
            return subject.ToUpper();
        }

    }
}
