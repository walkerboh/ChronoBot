﻿using System;
using System.Collections.Generic;
using Discord;
using Newtonsoft.Json;

namespace ChronoBot.Entities.ChronoGG
{
    public class Sale
    {
        public string Name { get; set; }
        public string Url { get; set; }

        [JsonProperty("unique_url")]
        public string UniqueUrl { get; set; }

        [JsonProperty("steam_url")]
        public string SteamUrl { get; set; }

        [JsonProperty("og_image")]
        public string OgImage { get; set; }
        public string[] Platforms { get; set; }

        [JsonProperty("promo_image")]
        public string PromoImage { get; set; }

        [JsonProperty("normal_price")]
        public decimal NormalPrice { get; set; }
        public string Discount { get; set; }

        [JsonProperty("sale_price")]
        public decimal SalePrice { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }
        public Item[] Items { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }
        public string Currency { get; set; }

        public class Item
        {
            public string Type { get; set; }
            public string Id { get; set; }
        }

        public Embed ToEmbed()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{Name}")
                .WithUrl(UniqueUrl)
                .WithImageUrl(PromoImage)
                .AddInlineField("Sale Price", SalePrice)
                .AddInlineField("Original Price", NormalPrice)
                .AddInlineField("Discount", Discount)
                .AddInlineField("Platforms", CleanPlatforms(Platforms))
                .WithFooter($"Sale ends {EndDate.ToUniversalTime().ToString("M/d/yy H:mm K")}");

            return embed.Build();
        }

        private object CleanPlatforms(string[] platforms)
        {
            List<string> cleanedStrings = new List<string>();

            foreach (string platform in platforms)
            {
                switch (platform)
                {
                    case "windows":
                        cleanedStrings.Add("Windows");
                        break;
                    case "macos":
                        cleanedStrings.Add("MacOS");
                        break;
                    case "linux":
                        cleanedStrings.Add("Linux");
                        break;
                }
            }

            return String.Join(", ", cleanedStrings);
        }
    }
}
