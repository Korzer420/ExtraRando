using ExtraRando.Data;
using ExtraRando.ModInterop.ItemChangerInterop;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using KorzUtils.Helper;
using Modding;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtraRando.ModInterop.Randomizer;

public static class RandoInterop
{
    #region Properties

    public static List<(string, string)> JunkPlacements { get; set; } = new();

    #endregion

    #region Event handler

    private static void ApplySettings(RequestBuilder builder)
    {
        if (!ExtraRando.Instance.Settings.Enabled)
            return;

        if (ExtraRando.Instance.Settings.SplitShadeCloak && builder.gs.PoolSettings.Skills)
        {
            builder.RemoveItemByName(ItemNames.Shade_Cloak);
            builder.RemoveItemByName(ItemNames.Split_Shade_Cloak);
            builder.RemoveItemByName(ItemNames.Mothwing_Cloak);
            builder.RemoveItemByName(ItemNames.Left_Mothwing_Cloak);
            builder.RemoveItemByName(ItemNames.Right_Mothwing_Cloak);

            // If duplicates are enabled the items are added with the placeholder prefix. We want to remove them as well.
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Shade_Cloak}");
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Split_Shade_Cloak}");
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Mothwing_Cloak}");
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Left_Mothwing_Cloak}");
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Right_Mothwing_Cloak}");

            string startItem = null;
            if (builder.IsAtStart(ItemNames.Mothwing_Cloak))
                startItem = ItemNames.Mothwing_Cloak;
            else if (builder.IsAtStart(ItemNames.Left_Mothwing_Cloak))
                startItem = ItemNames.Left_Mothwing_Cloak;
            else if (builder.IsAtStart(ItemNames.Right_Mothwing_Cloak))
                startItem = ItemNames.Right_Mothwing_Cloak;
            if (ExtraRando.Instance.Settings.ScarceItemPool)
            {
                if (!string.IsNullOrEmpty(startItem))
                {
                    builder.RemoveFromStart(startItem);
                    string startDash = startItem == ItemNames.Mothwing_Cloak
                        ? builder.rng.Next(0, 2) == 0
                            ? ItemManager.Left_Cloak
                            : ItemManager.Right_Cloak
                        : startItem == ItemNames.Left_Mothwing_Cloak
                            ? ItemManager.Left_Cloak
                            : ItemManager.Right_Cloak;
                    string randomizableDash = startDash == ItemManager.Left_Cloak
                        ? ItemManager.Right_Cloak
                        : ItemManager.Left_Cloak;
                    builder.AddToStart(startDash);
                    builder.AddItemByName(randomizableDash);
                    if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeBoth
                        || (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRight && randomizableDash == ItemManager.Right_Cloak)
                        || (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeLeft && randomizableDash == ItemManager.Left_Cloak)
                        || (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRight && builder.rng.Next(0, 2) == 0))
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{randomizableDash}");
                }
                else
                {
                    builder.AddItemByName(ItemManager.Left_Cloak);
                    builder.AddItemByName(ItemManager.Right_Cloak);
                    if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeBoth)
                    {
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Left_Cloak}");
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Right_Cloak}");
                    }
                    else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRight)
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Right_Cloak}");
                    else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeLeft)
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Left_Cloak}");
                    else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRandom)
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{(builder.rng.Next(0, 2) == 0 ? ItemManager.Left_Cloak : ItemManager.Right_Cloak)}");
                }

                builder.EditItemRequest(ItemManager.Left_Cloak, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Left_Cloak,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
                builder.EditItemRequest(ItemManager.Right_Cloak, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Right_Cloak,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(startItem))
                {
                    builder.RemoveFromStart(startItem);
                    string startDash = startItem == ItemNames.Mothwing_Cloak
                        ? builder.rng.Next(0, 2) == 0
                            ? ItemManager.Progressive_Left_Cloak
                            : ItemManager.Progressive_Right_Cloak
                        : startItem == ItemNames.Left_Mothwing_Cloak
                            ? ItemManager.Progressive_Left_Cloak
                            : ItemManager.Progressive_Right_Cloak;
                    string randomizableDash = startDash == ItemManager.Progressive_Left_Cloak
                        ? ItemManager.Progressive_Right_Cloak
                        : ItemManager.Progressive_Left_Cloak;
                    builder.AddToStart(startDash);
                    builder.AddItemByName(startDash);
                    builder.AddItemByName(randomizableDash, 2);
                }
                else
                {
                    builder.AddItemByName(ItemManager.Progressive_Left_Cloak, 2);
                    builder.AddItemByName(ItemManager.Progressive_Right_Cloak, 2);
                }

                if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeBoth)
                {
                    builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Progressive_Left_Cloak}");
                    builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Progressive_Right_Cloak}");
                }
                else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRight)
                    builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Progressive_Right_Cloak}");
                else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeLeft)
                    builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Progressive_Left_Cloak}");
                else if (builder.gs.DuplicateItemSettings.SplitCloakHandling == DuplicateItemSettings.SplitItemSetting.DupeRandom)
                    builder.AddItemByName($"{PlaceholderItem.Prefix}{(builder.rng.Next(0, 2) == 0 ? ItemManager.Progressive_Left_Cloak : ItemManager.Progressive_Right_Cloak)}");

                builder.EditItemRequest(ItemManager.Progressive_Left_Cloak, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Progressive_Left_Cloak,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
                builder.EditItemRequest(ItemManager.Progressive_Right_Cloak, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Progressive_Right_Cloak,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
            }
        }
        if (ExtraRando.Instance.Settings.RandomizeHotSprings)
        {
            builder.AddItemByName(ItemManager.Hot_Spring_Water, 6);
            builder.EditItemRequest(ItemManager.Hot_Spring_Water, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Hot_Spring_Water,
                    Pool = "Soul",
                    PriceCap = 50
                };
            });

            builder.AddLocationByName(ItemManager.Colosseum_Hot_Spring);
            builder.AddLocationByName(ItemManager.Bathhouse_Hot_Spring);
            builder.AddLocationByName(ItemManager.Crossroads_Hot_Spring);
            builder.AddLocationByName(ItemManager.Deepnest_Hot_Spring);
            builder.AddLocationByName(ItemManager.Lower_Godhome_Hot_Spring);
            builder.AddLocationByName(ItemManager.Upper_Godhome_Hot_Spring);
        }
        if (ExtraRando.Instance.Settings.RandomizeColoAccess)
        {
            builder.AddItemByName(ItemManager.Colo_Ticket_Bronze);
            builder.AddItemByName(ItemManager.Colo_Ticket_Silver);
            builder.AddItemByName(ItemManager.Colo_Ticket_Gold);

            builder.EditItemRequest(ItemManager.Colo_Ticket_Bronze, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Colo_Ticket_Bronze,
                    Pool = "Key",
                    PriceCap = 333
                };
            });
            builder.EditItemRequest(ItemManager.Colo_Ticket_Silver, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Colo_Ticket_Silver,
                    Pool = "Key",
                    PriceCap = 666
                };
            });
            builder.EditItemRequest(ItemManager.Colo_Ticket_Gold, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Colo_Ticket_Gold,
                    Pool = "Key",
                    PriceCap = 1000
                };
            });
        }
        if (ExtraRando.Instance.Settings.RandomizePantheonAccess)
        {
            builder.AddItemByName(ItemManager.Pantheon_Access_Master);
            builder.AddItemByName(ItemManager.Pantheon_Access_Artist);
            builder.AddItemByName(ItemManager.Pantheon_Access_Sage);
            builder.AddItemByName(ItemManager.Pantheon_Access_Knight);
            builder.AddItemByName(ItemManager.Pantheon_Access_Hallownest);

            builder.EditItemRequest(ItemManager.Pantheon_Access_Master, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Pantheon_Access_Master,
                    Pool = "Key",
                    PriceCap = 100
                };
            });
            builder.EditItemRequest(ItemManager.Pantheon_Access_Artist, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Pantheon_Access_Artist,
                    Pool = "Key",
                    PriceCap = 200
                };
            });
            builder.EditItemRequest(ItemManager.Pantheon_Access_Sage, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Pantheon_Access_Sage,
                    Pool = "Key",
                    PriceCap = 300
                };
            });
            builder.EditItemRequest(ItemManager.Pantheon_Access_Knight, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Pantheon_Access_Knight,
                    Pool = "Key",
                    PriceCap = 400
                };
            });
            builder.EditItemRequest(ItemManager.Pantheon_Access_Hallownest, info =>
            {
                info.getItemDef = () => new()
                {
                    MajorItem = false,
                    Name = ItemManager.Pantheon_Access_Hallownest,
                    Pool = "Key",
                    PriceCap = 500
                };
            });

            builder.AddLocationByName(ItemManager.Pantheon_Master);
            builder.AddLocationByName(ItemManager.Pantheon_Artist);
            builder.AddLocationByName(ItemManager.Pantheon_Sage);
            builder.AddLocationByName(ItemManager.Pantheon_Knight);
            builder.AddLocationByName(ItemManager.Pantheon_Hallownest);
        }
        if (ExtraRando.Instance.Settings.RandomizeButt)
        {
            builder.AddItemByName(ItemManager.Bardoon_Butt_Smack);
            builder.EditItemRequest(ItemManager.Bardoon_Butt_Smack, info =>
            {
                info.getItemDef = () => new()
                {
                    Name = ItemManager.Bardoon_Butt_Smack,
                    Pool = "KEY",
                    PriceCap = 10000
                };
            });

            builder.AddLocationByName(ItemManager.Bardoon_Butt);
            builder.EditLocationRequest(ItemManager.Bardoon_Butt, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = ItemManager.Bardoon_Butt,
                    SceneName = "Deepnest_East_04"
                };
            });
        }
        if (ExtraRando.Instance.Settings.SplitFireball && builder.gs.PoolSettings.Skills)
        {
            builder.AddLocationByName(ItemManager.Split_Vengeful_Spirit);
            builder.AddLocationByName(ItemManager.Split_Shade_Soul);

            builder.EditLocationRequest(ItemManager.Split_Vengeful_Spirit, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = ItemManager.Split_Vengeful_Spirit,
                    SceneName = "Crossroads_ShamanTemple"
                };
            });
            builder.EditLocationRequest(ItemManager.Split_Shade_Soul, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = ItemManager.Split_Shade_Soul,
                    SceneName = "Ruins1_31b"
                };
            });

            builder.RemoveItemByName(ItemNames.Vengeful_Spirit);
            builder.RemoveItemByName(ItemNames.Shade_Soul);
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Vengeful_Spirit}");
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Shade_Soul}");

            if (!ExtraRando.Instance.Settings.ScarceItemPool || builder.gs.CursedSettings.RemoveSpellUpgrades)
            {
                // Level 1 spell handling
                if (builder.IsAtStart(ItemNames.Vengeful_Spirit))
                {
                    builder.RemoveFromStart(ItemNames.Vengeful_Spirit);
                    bool addLeft = builder.rng.Next(0, 2) == 0;
                    builder.AddToStart(addLeft ? ItemManager.Left_Vengeful_Spirit : ItemManager.Right_Vengeful_Spirit);
                    builder.AddItemByName(!addLeft ? ItemManager.Left_Vengeful_Spirit : ItemManager.Right_Vengeful_Spirit);
                }
                else
                {
                    builder.AddItemByName(ItemManager.Left_Vengeful_Spirit);
                    builder.AddItemByName(ItemManager.Right_Vengeful_Spirit);
                }
                builder.EditItemRequest(ItemManager.Left_Vengeful_Spirit, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Left_Vengeful_Spirit,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
                builder.EditItemRequest(ItemManager.Right_Vengeful_Spirit, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Right_Vengeful_Spirit,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });

                if (builder.gs.DuplicateItemSettings.LevelOneSpells)
                {
                    if (!builder.IsAtStart(ItemManager.Left_Vengeful_Spirit))
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Left_Vengeful_Spirit}");
                    if (!builder.IsAtStart(ItemManager.Right_Vengeful_Spirit))
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Right_Vengeful_Spirit}");
                }

                // Level 2 spell handling
                if (!builder.gs.CursedSettings.RemoveSpellUpgrades)
                {
                    builder.AddItemByName(ItemManager.Left_Shade_Soul);
                    builder.AddItemByName(ItemManager.Right_Shade_Soul);

                    if (builder.gs.DuplicateItemSettings.LevelTwoSpells)
                    {
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Left_Shade_Soul}");
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Right_Shade_Soul}");
                    }
                    builder.EditItemRequest(ItemManager.Left_Shade_Soul, info =>
                    {
                        info.getItemDef = () => new()
                        {
                            MajorItem = true,
                            Name = ItemManager.Left_Shade_Soul,
                            Pool = "Skill",
                            PriceCap = 500
                        };
                    });
                    builder.EditItemRequest(ItemManager.Right_Shade_Soul, info =>
                    {
                        info.getItemDef = () => new()
                        {
                            MajorItem = true,
                            Name = ItemManager.Right_Shade_Soul,
                            Pool = "Skill",
                            PriceCap = 500
                        };
                    });
                }
            }
            else
            {
                if (builder.IsAtStart(ItemNames.Vengeful_Spirit))
                {
                    builder.RemoveFromStart(ItemNames.Vengeful_Spirit);
                    bool startWithLeft = builder.rng.Next(0, 2) == 0;
                    builder.AddToStart(startWithLeft ? ItemManager.Left_Fireball : ItemManager.Right_Fireball);
                    builder.AddItemByName(!startWithLeft ? ItemManager.Left_Fireball : ItemManager.Right_Fireball);
                }
                else
                {
                    builder.AddItemByName(ItemManager.Left_Fireball);
                    builder.AddItemByName(ItemManager.Right_Fireball);
                }

                builder.EditItemRequest(ItemManager.Left_Fireball, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Left_Fireball,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });
                builder.EditItemRequest(ItemManager.Right_Fireball, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = true,
                        Name = ItemManager.Right_Fireball,
                        Pool = "Skill",
                        PriceCap = 500
                    };
                });

                if (builder.gs.DuplicateItemSettings.LevelOneSpells || builder.gs.DuplicateItemSettings.LevelTwoSpells)
                {
                    if (!builder.IsAtStart(ItemManager.Left_Fireball))
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Left_Fireball}");
                    if (!builder.IsAtStart(ItemManager.Right_Fireball))
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Right_Fireball}");
                }
            }
        }
        if (ExtraRando.Instance.Settings.RandomizeAwfulLocations)
        {
            builder.AddLocationByName(ItemManager.GPZ_10);
            builder.AddLocationByName(ItemManager.White_Defender_5);
            builder.EditLocationRequest(ItemManager.GPZ_10, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = ItemManager.GPZ_10,
                    SceneName = "Room_Bretta_Basement"
                };
            });
            builder.EditLocationRequest(ItemManager.White_Defender_5, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = ItemManager.White_Defender_5,
                    SceneName = "Waterways_15"
                };
            });
        }
        if (ExtraRando.Instance.Settings.BlockEarlyGameStags)
            builder.AddToVanilla(ItemManager.Dirtmouth_Stag_Key, ItemManager.Dirtmouth_Stag_Door);

        if (ExtraRando.Instance.Settings.UseKeyring && builder.gs.PoolSettings.Keys)
        {
            builder.RemoveItemByName(ItemNames.Simple_Key);
            builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Simple_Key}");

            if (builder.IsAtStart(ItemNames.Simple_Key))
            {
                builder.RemoveFromStart(ItemNames.Simple_Key);
                builder.AddToStart(ItemManager.Key_Ring);
            }
            else
            {
                builder.AddItemByName(ItemManager.Key_Ring);
                if (builder.gs.DuplicateItemSettings.SimpleKeyHandling != DuplicateItemSettings.SimpleKeySetting.NoDupe)
                    builder.AddItemByName(ItemManager.Key_Ring);
                builder.EditItemRequest(ItemManager.Key_Ring, info =>
                    {
                        info.getItemDef = () => new()
                        {
                            MajorItem = true,
                            PriceCap = 500,
                            Name = ItemManager.Key_Ring,
                            Pool = "Key"
                        };
                    });


            }
        }
    }

    private static void ApplySpecialSettings(RequestBuilder builder)
    {
        JunkPlacements.Clear();
        if (!ExtraRando.Instance.Settings.Enabled)
            return;

        if (ExtraRando.Instance.Settings.ScarceItemPool)
        {
            List<ItemGroupBuilder> availablePools = new();
            foreach (StageBuilder stage in builder.Stages)
                foreach (ItemGroupBuilder itemGroup in stage.Groups.Where(x => x is ItemGroupBuilder).Select(x => x as ItemGroupBuilder))
                    if (!availablePools.Contains(itemGroup))
                        availablePools.Add(itemGroup);
            if (builder.gs.PoolSettings.Skills)
            {
                if (!builder.gs.CursedSettings.RemoveSpellUpgrades)
                {
                    builder.RemoveItemByName(ItemNames.Vengeful_Spirit);
                    builder.RemoveItemByName(ItemNames.Howling_Wraiths);
                    builder.RemoveItemByName(ItemNames.Desolate_Dive);
                    builder.RemoveItemByName(ItemNames.Abyss_Shriek);
                    builder.RemoveItemByName(ItemNames.Descending_Dark);
                    builder.RemoveItemByName(ItemNames.Shade_Soul);

                    // If duplicates are enabled the items are added with the placeholder prefix. We want to remove them as well.
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Vengeful_Spirit}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Howling_Wraiths}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Abyss_Shriek}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Shade_Cloak}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Descending_Dark}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Shade_Soul}");

                    if (builder.IsAtStart(ItemNames.Howling_Wraiths))
                    {
                        builder.RemoveFromStart(ItemNames.Howling_Wraiths);
                        builder.AddToStart(ItemManager.Scream_Spell);
                    }
                    else
                        builder.AddItemByName(ItemManager.Scream_Spell);

                    if (builder.IsAtStart(ItemNames.Desolate_Dive))
                    {
                        builder.RemoveFromStart(ItemNames.Desolate_Dive);
                        builder.AddToStart(ItemManager.Dive_Spell);
                    }
                    else
                        builder.AddItemByName(ItemManager.Dive_Spell);


                    if (builder.gs.DuplicateItemSettings.LevelOneSpells || builder.gs.DuplicateItemSettings.LevelTwoSpells)
                    {
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Dive_Spell}");
                        builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Scream_Spell}");
                    }
                    // The scarce option is already applied in the split fireball sections, therefore we can skip it here.
                    if (!ExtraRando.Instance.Settings.SplitFireball)
                        if (builder.IsAtStart(ItemNames.Vengeful_Spirit))
                        {
                            builder.RemoveFromStart(ItemNames.Vengeful_Spirit);
                            builder.AddToStart(ItemManager.Fireball_Spell);
                        }
                        else
                        {
                            builder.AddItemByName(ItemManager.Fireball_Spell);
                            if (builder.gs.DuplicateItemSettings.LevelOneSpells || builder.gs.DuplicateItemSettings.LevelTwoSpells)
                                builder.AddItemByName($"{PlaceholderItem.Prefix}{ItemManager.Fireball_Spell}");
                        }
                }

                if (!ExtraRando.Instance.Settings.SplitShadeCloak)
                {
                    builder.RemoveItemByName(ItemNames.Left_Mothwing_Cloak);
                    builder.RemoveItemByName(ItemNames.Right_Mothwing_Cloak);
                    builder.RemoveItemByName(ItemNames.Split_Shade_Cloak);
                    builder.RemoveItemByName(ItemNames.Shade_Cloak);
                    builder.RemoveItemByName(ItemNames.Mothwing_Cloak);

                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Shade_Cloak}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Split_Shade_Cloak}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Mothwing_Cloak}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Left_Mothwing_Cloak}");
                    builder.RemoveItemByName($"{PlaceholderItem.Prefix}{ItemNames.Right_Mothwing_Cloak}");
                    if (builder.gs.NoveltySettings.SplitCloak)
                    {
                        // We can just utilize the packed split shade cloak items since the user will not be able to dash in the opposite direction anyway.
                        builder.AddItemByName(ItemManager.Left_Cloak);
                        builder.AddItemByName(ItemManager.Right_Cloak);
                        if (builder.gs.DuplicateItemSettings.MothwingCloak)
                        {
                            builder.AddItemByName(ItemManager.Left_Cloak);
                            builder.AddItemByName(ItemManager.Right_Cloak);
                        }
                    }
                    else
                    {
                        builder.AddItemByName(ItemManager.Cloak);
                        if (builder.gs.DuplicateItemSettings.MothwingCloak)
                            builder.AddItemByName(ItemManager.Cloak);
                    }
                }
            }
            List<string> itemsToRemove = new();

            if (!builder.gs.CursedSettings.ReplaceJunkWithOneGeo)
            {
                if (builder.gs.PoolSettings.MaskShards)
                    switch (builder.gs.MiscSettings.MaskShards)
                    {
                        case MiscSettings.MaskShardType.FourShardsPerMask:
                            itemsToRemove.AddRange(Enumerable.Range(0, 2).Select(x => ItemNames.Full_Mask));
                            break;
                        case MiscSettings.MaskShardType.TwoShardsPerMask:
                            itemsToRemove.AddRange(Enumerable.Range(0, 4).Select(x => ItemNames.Double_Mask_Shard));
                            break;
                        default:
                            itemsToRemove.AddRange(Enumerable.Range(0, 8).Select(x => ItemNames.Mask_Shard));
                            break;
                    }

                if (builder.gs.PoolSettings.VesselFragments)
                    switch (builder.gs.MiscSettings.VesselFragments)
                    {
                        case MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel:
                            itemsToRemove.Add(ItemNames.Full_Soul_Vessel);
                            break;
                        case MiscSettings.VesselFragmentType.TwoFragmentsPerVessel:
                            itemsToRemove.Add(ItemNames.Vessel_Fragment);
                            itemsToRemove.Add(ItemNames.Double_Vessel_Fragment);
                            break;
                        default:
                            itemsToRemove.AddRange(Enumerable.Range(0, 8).Select(x => ItemNames.Vessel_Fragment));
                            break;
                    }

                if (builder.gs.PoolSettings.CharmNotches || builder.gs.MiscSettings.SalubraNotches == MiscSettings.SalubraNotchesSetting.Randomized)
                    itemsToRemove.Add(ItemNames.Charm_Notch);
            }

            while (itemsToRemove.Any())
            {
                ItemGroupBuilder selectedBuilder = availablePools.FirstOrDefault(x => x.Items.EnumerateWithMultiplicity().Contains(itemsToRemove[0]));
                selectedBuilder?.Items.Remove(itemsToRemove[0], 1);
                itemsToRemove.RemoveAt(0);
            }
        }
        if (ExtraRando.Instance.Settings.AddHintMarkers)
        {
            if (ExtraRando.Instance.Settings.JunkItemHints > 0)
            {
                builder.AddItemByName(ItemManager.Scarab_Marker_Hint, ExtraRando.Instance.Settings.JunkItemHints);
                builder.EditItemRequest(ItemManager.Scarab_Marker_Hint, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = false,
                        PriceCap = 50,
                        Name = ItemManager.Scarab_Marker_Hint,
                        Pool = "Map"
                    };
                });
            }
            if (ExtraRando.Instance.Settings.PotentialItemHints > 0)
            {
                builder.AddItemByName(ItemManager.Gleaming_Marker_Hint, ExtraRando.Instance.Settings.PotentialItemHints);
                builder.EditItemRequest(ItemManager.Gleaming_Marker_Hint, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = false,
                        PriceCap = 50,
                        Name = ItemManager.Gleaming_Marker_Hint,
                        Pool = "Map"
                    };
                });
            }
            if (ExtraRando.Instance.Settings.UsefulItemHints > 0)
            {
                builder.AddItemByName(ItemManager.Token_Marker_Hint, ExtraRando.Instance.Settings.UsefulItemHints);
                builder.EditItemRequest(ItemManager.Token_Marker_Hint, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = false,
                        PriceCap = 50,
                        Name = ItemManager.Token_Marker_Hint,
                        Pool = "Map"
                    };
                });
            }
            if (ExtraRando.Instance.Settings.RandomLocationHints > 0)
            {
                builder.AddItemByName(ItemManager.Shell_Marker_Hint, ExtraRando.Instance.Settings.RandomLocationHints);
                builder.EditItemRequest(ItemManager.Shell_Marker_Hint, info =>
                {
                    info.getItemDef = () => new()
                    {
                        MajorItem = false,
                        PriceCap = 50,
                        Name = ItemManager.Shell_Marker_Hint,
                        Pool = "Map"
                    };
                });
            }
        }
        if (ExtraRando.Instance.Settings.EnforceJunkLocations)
        {
            string junkFile = Path.Combine(Path.GetDirectoryName(typeof(ExtraRando).Assembly.Location), "JunkLocations.txt");
            if (!File.Exists(junkFile))
            {
                LogHelper.Write<ExtraRando>("Couldn't find \"JunkLocations.txt\" file in ExtraRando directory. To junk locations, provide the file.", KorzUtils.Enums.LogType.Warning, false);
                return;
            }

            List<string> locations = new();
            List<string> availableJunkItems = new();
            foreach (var item in builder.EnumerateItemGroups())
            {
                locations.AddRange(item.Locations.EnumerateDistinct());
                availableJunkItems.AddRange(item.Items.EnumerateWithMultiplicity().Where(x => x.StartsWith("Geo_Rock") || x == ItemNames.One_Geo
                    || x.StartsWith("Soul_Totem") || x == ItemNames.Soul_Refill || x.StartsWith("Geo_Chest")));
            }
            locations = locations.Distinct().ToList();

            string[] locationsToJunk = File.ReadAllLines(junkFile);
            if (locationsToJunk != null)
                foreach (string locationName in locationsToJunk)
                {
                    if (locationName.StartsWith("//"))
                        continue;
                    if (!locations.Contains(locationName) || Finder.GetLocation(locationName) == null)
                    {
                        LogHelper.Write<ExtraRando>($"Couldn't find location with name \"{locationName}\" to junk.", KorzUtils.Enums.LogType.Warning, false);
                        continue;
                    }
                    builder.RemoveLocationByName(locationName);
                    if (availableJunkItems.Any())
                    {
                        int chosenIndex = builder.rng.Next(0, availableJunkItems.Count);
                        builder.AddToVanilla(availableJunkItems[chosenIndex], locationName);
                        JunkPlacements.Add(new(availableJunkItems[chosenIndex], locationName));
                        availableJunkItems.RemoveAt(chosenIndex);
                    }
                    else
                    {
                        int rolledNumber = builder.rng.Next(0, 5);
                        string chosenJunk = rolledNumber switch
                        {
                            1 => ItemNames.One_Geo,
                            2 => ItemNames.Soul_Refill,
                            3 => ItemNames.Geo_Rock_Hive,
                            4 => ItemNames.Geo_Chest_False_Knight,
                            _ => ItemNames.Soul_Totem_Path_of_Pain,
                        };
                        builder.AddToVanilla(chosenJunk, locationName);
                        JunkPlacements.Add(new(chosenJunk, locationName));
                    }
                }
        }
    }

    private static void ModifyLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        if (!ExtraRando.Instance.Settings.Enabled)
            return;

        JsonLogicFormat jsonLogicFormat = new();
        using Stream macroFile = ResourceHelper.LoadResource<ExtraRando>("Randomizer.Macros.json");
        builder.DeserializeFile(LogicFileType.Macros, jsonLogicFormat, macroFile);

        using Stream waypointStream = ResourceHelper.LoadResource<ExtraRando>("Randomizer.Waypoints.json");
        builder.DeserializeFile(LogicFileType.Waypoints, jsonLogicFormat, waypointStream);

        using Stream logicFile = ResourceHelper.LoadResource<ExtraRando>("Randomizer.Logic.json");
        builder.DeserializeFile(LogicFileType.Locations, jsonLogicFormat, logicFile);

        if (ExtraRando.Instance.Settings.SplitFireball)
        {
            using Stream substituteFile = ResourceHelper.LoadResource<ExtraRando>("Randomizer.Substitutions.json");
            builder.DeserializeFile(LogicFileType.LogicSubst, jsonLogicFormat, substituteFile);

            builder.LogicLookup.Add(ItemManager.Split_Vengeful_Spirit, builder.LogicLookup["Vengeful_Spirit"]);
            builder.LogicLookup.Add(ItemManager.Split_Shade_Soul, builder.LogicLookup["Shade_Soul"]);

            builder.GetOrAddTerm("FIREBALLLEFT");
            builder.GetOrAddTerm("FIREBALLRIGHT");

            builder.AddItem(new MultiItem(ItemManager.Left_Vengeful_Spirit,
            [
                new(builder.GetTerm("FIREBALL"), 1),
                new(builder.GetTerm("FIREBALLLEFT"), 1),
                new(builder.GetTerm("SPELLS"), 1)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Left_Shade_Soul,
            [
                new(builder.GetTerm("FIREBALL"), 1),
                new(builder.GetTerm("FIREBALLLEFT"), 1),
                new(builder.GetTerm("SPELLS"), 1)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Right_Vengeful_Spirit,
            [
                new(builder.GetTerm("FIREBALL"), 1),
                new(builder.GetTerm("FIREBALLRIGHT"), 1),
                new(builder.GetTerm("SPELLS"), 1)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Right_Shade_Soul,
            [
                new(builder.GetTerm("FIREBALL"), 1),
                new(builder.GetTerm("FIREBALLRIGHT"), 1),
                new(builder.GetTerm("SPELLS"), 1)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Left_Fireball,
            [
                new(builder.GetTerm("FIREBALL"), 2),
                new(builder.GetTerm("FIREBALLLEFT"), 2),
                new(builder.GetTerm("SPELLS"), 2)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Right_Fireball,
            [
                new(builder.GetTerm("FIREBALL"), 2),
                new(builder.GetTerm("FIREBALLRIGHT"), 2),
                new(builder.GetTerm("SPELLS"), 2)
            ]));
        }
        if (ExtraRando.Instance.Settings.ScarceItemPool)
        {
            builder.AddItem(new MultiItem(ItemManager.Cloak,
            [
                new(builder.GetTerm("LEFTDASH"), 2),
                new(builder.GetTerm("RIGHTDASH"), 2)
            ]));
            builder.AddItem(new SingleItem(ItemManager.Left_Cloak, new(builder.GetTerm("LEFTDASH"), 2)));
            builder.AddItem(new SingleItem(ItemManager.Right_Cloak, new(builder.GetTerm("RIGHTDASH"), 2)));

            builder.AddItem(new MultiItem(ItemManager.Fireball_Spell,
            [
                new(builder.GetTerm("FIREBALL"), 2),
                new(builder.GetTerm("SPELLS"), 2)
            ]));
            builder.AddItem(new MultiItem(ItemManager.Dive_Spell,
            [
                new(builder.GetTerm("QUAKE"), 2),
                new(builder.GetTerm("SPELLS"), 2)

            ]));
            builder.AddItem(new MultiItem(ItemManager.Scream_Spell,
            [
                new(builder.GetTerm("SCREAM"), 2),
                new(builder.GetTerm("SPELLS"), 2)

            ]));
        }
        if (ExtraRando.Instance.Settings.SplitShadeCloak && settings.PoolSettings.Skills)
        {
            Term leftDash = builder.GetOrAddTerm("LEFTDASH");
            Term rightDash = builder.GetOrAddTerm("RIGHTDASH");
            builder.AddItem(new SingleItem(ItemManager.Progressive_Left_Cloak, new(leftDash, 1)));
            builder.AddItem(new SingleItem(ItemManager.Progressive_Right_Cloak, new(rightDash, 1)));

            // Adjust macros since normally this will be considered true if ANY shade cloak has been obtained.
            builder.DoMacroEdit(new("LEFTSHADOWDASH", "LEFTDASH>1"));
            builder.DoMacroEdit(new("RIGHTSHADOWDASH", "RIGHTDASH>1"));
        }
        if (ExtraRando.Instance.Settings.RandomizeHotSprings)
            builder.AddItem(new EmptyItem(ItemManager.Hot_Spring_Water));
        if (ExtraRando.Instance.Settings.AddHintMarkers)
        {
            builder.AddItem(new EmptyItem(ItemManager.Gleaming_Marker_Hint));
            builder.AddItem(new EmptyItem(ItemManager.Scarab_Marker_Hint));
            builder.AddItem(new EmptyItem(ItemManager.Shell_Marker_Hint));
            builder.AddItem(new EmptyItem(ItemManager.Token_Marker_Hint));
        }
        if (ExtraRando.Instance.Settings.RandomizeColoAccess)
        {
            Term coloTerm = builder.GetOrAddTerm("COLO_KEY_1");
            builder.AddItem(new SingleItem(ItemManager.Colo_Ticket_Bronze, new(coloTerm, 1)));

            coloTerm = builder.GetOrAddTerm("COLO_KEY_2");
            builder.AddItem(new SingleItem(ItemManager.Colo_Ticket_Silver, new(coloTerm, 1)));

            coloTerm = builder.GetOrAddTerm("COLO_KEY_3");
            builder.AddItem(new SingleItem(ItemManager.Colo_Ticket_Gold, new(coloTerm, 1)));

            builder.DoLogicEdit(new("Defeated_Colosseum_1", "(ORIG) + COLO_KEY_1"));
            builder.DoLogicEdit(new("Defeated_Colosseum_2", "(ORIG) + COLO_KEY_2"));

            if (builder.LogicLookup.ContainsKey("The_Glory_of_Being_a_Fool-Colosseum"))
                builder.DoLogicEdit(new("The_Glory_of_Being_a_Fool-Colosseum", "Room_Colosseum_01[left1] + Can_Replenish_Geo + (ANYCLAW | (SPICYCOMBATSKIPS + WINGS)) + COMBAT[Colosseum_2] + COLO_KEY_3 + (READ ? TRUE)"));
        }
        if (ExtraRando.Instance.Settings.RandomizePantheonAccess)
        {
            Term pantheonTerm = builder.GetOrAddTerm("PANTHEON_KEY_1");
            builder.AddItem(new SingleItem(ItemManager.Pantheon_Access_Master, new(pantheonTerm, 1)));

            pantheonTerm = builder.GetOrAddTerm("PANTHEON_KEY_2");
            builder.AddItem(new SingleItem(ItemManager.Pantheon_Access_Artist, new(pantheonTerm, 1)));

            pantheonTerm = builder.GetOrAddTerm("PANTHEON_KEY_3");
            builder.AddItem(new SingleItem(ItemManager.Pantheon_Access_Sage, new(pantheonTerm, 1)));

            pantheonTerm = builder.GetOrAddTerm("PANTHEON_KEY_4");
            builder.AddItem(new SingleItem(ItemManager.Pantheon_Access_Knight, new(pantheonTerm, 1)));

            pantheonTerm = builder.GetOrAddTerm("PANTHEON_KEY_5");
            builder.AddItem(new SingleItem(ItemManager.Pantheon_Access_Hallownest, new(pantheonTerm, 1)));
        }
        if (ExtraRando.Instance.Settings.RandomizeButt)
            builder.AddItem(new EmptyItem(ItemManager.Bardoon_Butt_Smack));
        if (ExtraRando.Instance.Settings.RandomizeAwfulLocations)
        {
            // As this settings allows White Defender to be accessable with one dreamer we need to adjust that.
            builder.DoSubst(new("Boss_Essence-White_Defender", "DREAMER3", "DREAMER"));
            builder.DoSubst(new("Defeated_White_Defender", "DREAMER3", "DREAMER"));
            builder.AddLogicDef(new(ItemManager.White_Defender_5, "Defeated_White_Defender"));
            builder.LogicLookup[ItemManager.White_Defender_5] = builder.LogicLookup["Boss_Essence-White_Defender"];

            builder.AddLogicDef(new(ItemManager.GPZ_10, "Defeated_Grey_Prince_Zote"));
            builder.LogicLookup[ItemManager.GPZ_10] = builder.LogicLookup["Boss_Essence-Grey_Prince_Zote"];
        }
        if (ExtraRando.Instance.Settings.BlockEarlyGameStags)
        {
            builder.DoLogicEdit(new("Town[door_station]", "(ORIG) + Dirtmouth_Stag_Key | Town[door_station]"));
            builder.DoLogicEdit(new("Room_Town_Stag_Station[left1]", "(ORIG) + Dirtmouth_Stag_Key | Room_Town_Stag_Station[left1]"));
            builder.DoLogicEdit(new("Crossroads_47[right1]", "(ORIG) + Dirtmouth_Stag_Key | Crossroads_47[right1]"));
            Term stagTerm = builder.GetOrAddTerm("Dirtmouth_Stag_Key");
            builder.AddItem(new BoolItem(ItemManager.Dirtmouth_Stag_Key, stagTerm));
        }
        if (ExtraRando.Instance.Settings.UseKeyring)
            builder.AddItem(new SingleItem(ItemManager.Key_Ring, new(builder.GetTerm("SIMPLE"), 4)));
        if (ExtraRando.Instance.Settings.UseVictoryConditions)
        {
            List<string> conditions = [];
            foreach (var usedCondition in ExtraRando.Instance.Settings.VictoryConditions.Where(x => x.Value > 0))
            {
                IVictoryCondition condition = VictoryModule.AvailableConditions.First(x => x.GetType().Name == usedCondition.Key);
                string termValue = condition.PrepareLogic(builder);
                conditions.Add($"{termValue}>{usedCondition.Value - 1}");
            }
            if (conditions.Count > 0)
            {
                string logic = ExtraRando.Instance.Settings.ConditionHandling == Enums.VictoryConditionHandling.Any
                    ? conditions.Aggregate((x, y) => $"{x} | {y}")
                    : conditions.Aggregate((x, y) => $"{x} + {y}");
                builder.DoLogicEdit(new("Opened_Black_Egg_Temple", $"Room_temple[left1] + ({logic})"));
            }
        }
    }

    private static void CheckForNoLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        if (!ExtraRando.Instance.Settings.Enabled || !ExtraRando.Instance.Settings.NoLogic)
            return;
        foreach (string key in builder.LogicLookup.Keys.ToList())
            builder.DoLogicEdit(new(key, "TRUE"));
    }

    private static void WriteSettings(LogArguments arg1, TextWriter textWriter)
    {
        textWriter.WriteLine("ExtraRando settings");
        using Newtonsoft.Json.JsonTextWriter jsonTextWriter = new(textWriter) { CloseOutput = false, };
        RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jsonTextWriter, ExtraRando.Instance.Settings);
        textWriter.WriteLine();
    }

    private static int RandoController_OnCalculateHash(RandoController arg1, int arg2)
    {
        if (!ExtraRando.Instance.Settings.Enabled)
            return 0;
        int hashModifier = 1;
        if (ExtraRando.Instance.Settings.NoLogic)
            hashModifier += 23;
        if (ExtraRando.Instance.Settings.BlockEarlyGameStags)
            hashModifier += 3;
        // In case no marker is placed but this setting is enabled.
        if (ExtraRando.Instance.Settings.AddHintMarkers)
            hashModifier += 5;
        if (ExtraRando.Instance.Settings.AddFixedHints)
            hashModifier += 51;

        if (ExtraRando.Instance.Settings.UseVictoryConditions && ExtraRando.Instance.Settings.VictoryConditions.Any(x => x.Value > 0))
        {
            hashModifier += 152;
            hashModifier += ExtraRando.Instance.Settings.VictoryConditions.Where(x => x.Value > 0).Select(x => x.Value).Sum();

            hashModifier += ExtraRando.Instance.Settings.WarpToCredits ? 634 : 0;
            hashModifier += ExtraRando.Instance.Settings.ConditionHandling == Enums.VictoryConditionHandling.Any ? 79 : 0;
        }
        return hashModifier;
    }

    private static void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        orig(self, permaDeath, bossRush);
        if (RandomizerMod.RandomizerMod.IsRandoSave && ExtraRando.Instance.Settings.Enabled)
        {
            List<AbstractPlacement> toAdd = [];
            if (ExtraRando.Instance.Settings.BlockEarlyGameStags)
            {
                AbstractPlacement placement = Finder.GetLocation(ItemManager.Dirtmouth_Stag_Door).Wrap();
                placement.Add(Finder.GetItem(ItemManager.Dirtmouth_Stag_Key));
                toAdd.Add(placement);
            }
            if (ExtraRando.Instance.Settings.EnforceJunkLocations && JunkPlacements.Any())
            {
                foreach ((string, string) requestPlacement in JunkPlacements)
                {
                    AbstractPlacement placement = Finder.GetLocation(requestPlacement.Item2)?.Wrap();
                    placement.Add(Finder.GetItem(requestPlacement.Item1));
                    toAdd.Add(placement);
                }
            }
            if (toAdd.Any())
                ItemChangerMod.AddPlacements(toAdd);
        }
    }

    private static void RandoController_OnExportCompleted(RandoController obj)
    {
        if (!ExtraRando.Instance.Settings.Enabled)
            return;
        if (ExtraRando.Instance.Settings.AddFixedHints && ItemChangerMod.Modules.Get<HintModule>() == null)
        {
            HintModule hintModule = ItemChangerMod.Modules.GetOrAdd<HintModule>();
            hintModule.AddHints();
        }
        if (ExtraRando.Instance.Settings.UseVictoryConditions)
        {
            VictoryModule module = ItemChangerMod.Modules.GetOrAdd<VictoryModule>();
            module.CombineCondition = ExtraRando.Instance.Settings.ConditionHandling == Enums.VictoryConditionHandling.All;
            module.WarpToCredits = ExtraRando.Instance.Settings.WarpToCredits;
            foreach (var usedCondition in ExtraRando.Instance.Settings.VictoryConditions)
            {
                Data.IVictoryCondition condition = VictoryModule.AvailableConditions.FirstOrDefault(x => x.GetType().Name == usedCondition.Key);
                if (condition != null)
                {
                    condition.RequiredAmount = usedCondition.Value;
                    if (condition.RequiredAmount > 0)
                        module.ActiveConditions.Add(condition);
                }
            }
        }
    }

    #endregion

    #region Methods

    internal static void Initialize()
    {
        RandoMenu.Initialize();
        RequestBuilder.OnUpdate.Subscribe(float.MaxValue - 1f, ApplySpecialSettings);
        RequestBuilder.OnUpdate.Subscribe(1050f, ApplySettings);
        RCData.RuntimeLogicOverride.Subscribe(1050f, ModifyLogic);
        RCData.RuntimeLogicOverride.Subscribe(float.MaxValue, CheckForNoLogic);

        SettingsLog.AfterLogSettings += WriteSettings;
        RandoController.OnCalculateHash += RandoController_OnCalculateHash;
        RandoController.OnExportCompleted += RandoController_OnExportCompleted;


        if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            HookRandoSettingsManager();
        CSL.CondensedSpoilerLogger.AddCategory("Other major items", () => ExtraRando.Instance.Settings.Enabled, new()
        {
            ItemManager.Cloak,
            ItemManager.Left_Cloak,
            ItemManager.Right_Cloak,
            ItemManager.Progressive_Left_Cloak,
            ItemManager.Progressive_Right_Cloak,
            ItemManager.Left_Fireball,
            ItemManager.Left_Vengeful_Spirit,
            ItemManager.Left_Shade_Soul,
            ItemManager.Right_Fireball,
            ItemManager.Right_Vengeful_Spirit,
            ItemManager.Right_Shade_Soul,
            ItemManager.Dive_Spell,
            ItemManager.Scream_Spell,
            ItemManager.Key_Ring
        });
        CSL.CondensedSpoilerLogger.AddCategory("Pantheon keys", () => ExtraRando.Instance.Settings.Enabled, new()
        {
            ItemManager.Pantheon_Access_Master,
            ItemManager.Pantheon_Access_Artist,
            ItemManager.Pantheon_Access_Sage,
            ItemManager.Pantheon_Access_Knight,
            ItemManager.Pantheon_Access_Hallownest
        });
        On.UIManager.StartNewGame += UIManager_StartNewGame;
    }

    private static void HookRandoSettingsManager()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new SimpleSettingsProxy<RandoSettings>(ExtraRando.Instance,
        RandoMenu.Instance.PassSettings,
        () => ExtraRando.Instance.Settings.Enabled ? ExtraRando.Instance.Settings : null));
    }

    #endregion
}
