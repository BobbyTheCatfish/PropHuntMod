

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PropHuntMod.Utils
{
    internal static class ExtraProps
    {
        public static Dictionary<string, string[]> props;

        public static void Init()
        {
            string[] cogwork =
            {
                "CC_metal__0001_28",
                "CC_metal__0005_24",
                "CC_metal__0010_19",
                "CC_metal__0012_17",
                "CC_metal__0023_6",
                "CC_metal__0025_4",
                "CC_metal__0027_2",
                "CC_destroyed_0000_1",
                "CC_destroyed_0001_1",
                "CC_destroyed_0002_1",
                "Cog_Choir",
                "cog_plat_spin_0008",
                "cog_plat_spin_0014",
                "cog_plat_spin_0015",
                "cog__0002_1",
                "song_city_pipes_0016_1",
                "automaton_corpses_basic",
                "cradle_floor__0002_1"
            };

            string[] abyss =
            {
                "abyss_0001_blue_root_10",
                "abyss_0002_blue_grass_02",
                "abyss_wall_small",
                "abyss_vine_packed_01",
                "Abyss_MID",
                "abyss_egg",
                "Loom_Room_0030_10_short_pillar",
                "loom_jars_pile"
            };

            string[] belltown =
            {
                "bell_old__0000_wall_large",
                "sc_bell_piles_0001_7",
                "bell_old__0005_wall_mid",
                "belltown_arch_set__0001_2",
                "belltown_arch_set__0000_2",
                "belltown_sign",
                "sign_board",
                "shell_bush_standard_0001_1",
                "shell_bush_standard_0000_2_white_flower",
                "shell_bush_standard_0001_1",
                "sc_junk_piles_small_0001_1",
                "pinhome_0010_1",
                "pinhome_0008_1",
                "bone_relic_room_0006_1",
                "bone_relic_room_0008_1",
                "bone_relic_room_0003_1",
                "pinhome_0012_1"
            };

            string[] bells =
            {
                "Hornet_Core__0092",
                "Hornet_Core__0093",
                "Hornet_Core__0094",
                "Hornet_Core__0095",
                "Hornet_Core__0096",
                "Hornet_Core__0097",
                "Hornet_Core__0099",
                "sc_bell_piles_0000_8",
                "sc_bell_piles_0001_7",
                "sc_bell_piles_0002_6",
                "sc_bell_piles_0004_4",
                "sc_bell_piles_0005_3",
                "sc_bell_piles_0007_1",
                "sc_bell_piles_base",
            };

            string[] huntersMarch =
            {
                "corpses_pilgrim",
                "ant_big_bone_circlet",
                "Ant_Bone_block",
                "Ant_Bone_Extras_0009_barrel_lower_02",
                "Ant_Bone_Extras_0010_barrel_lower_03",
                "Ant_Bone_Extras_0016_barrel_ring",
                "Ant_Bone_Extras_0021_heretic_spike_02",
                "Ant_Bone_Extras_0022_heretic_spike_01",
                "ant_break_branches_0001_1",
                "ant_bridge_pieces_0007_Layer-31",
                "ant_bridge_pieces_0012_Layer-35",
                "ant_bridge_pieces_0012_Layer-35_trapdoor",
                "ant_cage_trap_base",
                "ant_camp_0000_camp",
                "ant_camp_0001_camp",
                "Ant_Chapel_Beast_0000_1",
                "Ant_chief_throne_0001_1",
                "Ant_chief_throne",
                "ant_extras_branch_0000_3",
                "ant_extras_branch_0000_3_long",
                "ant_extras_branch_0002_1_full",
                "ant_theatre_extra_spires_0002_1",
                "Silkcatcher Plant"
            };

            string[] memorium =
            {
                "wilds_big_bush",
                "scorch_bone_0014_4",
                "shell_bush_vine_clump",
                "wilds_extras_0009_1",
                "wilds_big_bush",
                "Shellwood Bounce Bloom",
                "dock_b_0051_lore_sign_hang",
                "char_grass_03",
                "sc_drape_01 ",
                "Mossbone_cocoon_thinner",
                "bone_deep_0140_p",
                "moss__0062_v4",
                "Moss_Thick_Vine_standard0000",
                "SM_death_shellwood",
                "shell_bush_standard_0000_2",
                "conch_walls_0000_1_mid_angled",
                "Coral_red_branch_long_0000_2",
                "Coral_red_branch_long_0001_1",
                "pinned_bugs_",
                "pinned_bug_board",
                "materium_room_0002_1",
                "sc_junk_piles_small_0002_1",
                "song_flea_cage_furniture_0001_cage_front",
                "greymoor_cage_bits_0002_3",
                "Greymoor_craw_bits_0005_5",
                "sc_junk_piles_small_0001_1",
                "clover_goomba_husk",
                "moss__0003_bmoss",
                "lock_part",
                "map_machine_extra_0002_1",
                "clover_break_grass_0006_1",
                "Aspid__0007_1",
                "wp_cloth_02"
            };

            string[] bone =
            {
                "bone_bush",
                "Hornet_Core__0053_tent",
                "tent_pillows",
                "moss_table_break_0007_chair_01",
                "gold_pole_simple",
                "gold_spike_fence_long",
                "bone_deep_0163_q",
                "rosary_dish",
                "BP_dead",
                "Boneforest_breakables_0003_16",
                "SC_0052_sc_fence_01",
                "lake_vines_roof_0000_2",
                "docks__0008_3",
                "Shell Shard Fossil",
                "Hornet_Core__0097_bell_tunnel_01",
                "Hornet_Core__0096_bell_tunnel_01",
                "marrow_start_sign",
                "break_statues_0003_s",
                "break_statues_0004_s",
                "bell_old__0000_wall_large",
                "bell_old__0001_wall_large",
                "Bone_house_pieces_0006_4",
                "Bone_house_pieces_0002_8",
                "barrel_stacks_squat",
                "Thread Spinner",
                "Thread_Spool_spinner",
                "Bone_rubble_short",
                "Belltown_0030_statue",
                "Bone_house_pieces_0006_4",
                "cage_hang_standard",
                "stat horn small base",
                "sc_junk_piles_small_0",
                "bone_hut_sack_0004",
                "corpses_pilgrim",
                "Jar_break_extra",
                "weaver_heat_lamp",

            };

            string[] farfields =
            {
                "wilds_big_bush",
                "wilds_extras_0007_1",
                "ant_bridge_pieces_0007",
                "Shell Shard Fossil",
                "ant_camp_0002_camp",
                "ant_extras_branch_0000_3",
                "moss_breakable_0019_bone_tooth",
                "bone_deep_0140_p",
                "sc_extras_0013_sc_cart",
                "ant_camp_0000_camp",
                "break_statues_0003_s_stone_floor",
                "Silkcatcher Plant",
                "scorch_bone_0012_6",
                "_0001_umb_inflate_small",
                "_0009_umb_small",
                "ant_camp_0001_camp",
                "barrel_stacks_tall",
                "spool_base_up",
                "barrel_stacks_squat",
                "corpses_pilgrim",
                "Room_Umbrella_0015_9",
                "Room_Umbrella_0016_8",
                "Room_Umbrella_0017_7",
                "Room_Umbrella_0018",
                "weaver_lift_button_0002_1",
                "Loom_Room_0030"
            };

            string[] surface =
            {
                "abandoned_town_shellhomes_0002_1",
                "abandoned_town_jars_0004_1",
                "abandoned_town_shellhomes_floors_0001_1_corpse_broken",
                "cliffs_layered_07_0004_a"
            };

            string[] clover =
            {
                "moss__0037_p8",
                "Aspid_break_bushes",
                "moss__0043_p2",
                "clover_gate_arches_0004_1_top_map_board",
                "clover_lore_fountain_0000_1",
                "Lilypad Fly",
                "grove_pod_main0012",
                "Clover_Silk_Pod_Wide",
            };

            string[] coral =
            {
                "conch_walls_0004_1",
                "J_death0006",
                "Boneforest_breakables_0000_19",
                "bone_rubble_large_walls_0004_1",
                "Giant_Conch_bg_horn",
                "coral_shrub_break_large_red",
                "Coral Crust Lore Tablet",
                "corla_vine_short",
                "Coral_0038_Layer-102",
                "Hornet_cage_rect_0000_cage"
            };

            string[] wormways =
            {
                "kingdom_gate_0001_sand_dune_ground_short_mound",
                "song_gate_0001_1_short",
                "collector_eggs_0005_1",
                "bellway_tent_0002_1",
                "greymoor_cage_bits_0001_2"
            };
            string[] docks =
            {
                "DW_death0004",
                "wilds_big_bush",
                "dock_pipe__0026_1",
                "dock_pipe__0027_1",
                "dock_pipe__0028_1",
                "DF_death0004",
                "barrel_stacks_0001_2",
                "DS_death0004",
                "Belltown_floor_main"
            };

            string[] sinnersRoad =
            {
                "chef_room_0007_meat",
                "chef_room_0003_meat",
                "chef_room_0009_meat",
                "chef_room_0019_ladel",
                "kitchen_cauldrons_0001_1",
                "spool_base_up",
                "spool_base_side",
                "greymoor_sack",
                "greymoor_junk_small",
                "trader_extras_0000_1",
                "greymoor_cage_bits_0000_1"
            };

            string[] greymoor =
            {
                "_0039_grey_lumpy",
                "grey_0000_stone_mid_size_0001_1",
                "_0027_grey_"
            };

            props = new Dictionary<string, string[]>
            {
                { "Abandoned_town", surface },
                { "Abyss", abyss },
                { "Ant", huntersMarch },
                { "Arborium_", memorium },
                { "Belltown", belltown },
                { "Bellshrine", bells },
                { "Bellway", bells },
                { "Bone_East", farfields},
                { "Bone", bone },
                { "Clover", clover },
                { "Cog", cogwork },
                { "Coral", coral },
                { "Crawl", wormways },
                { "Docks", docks },
                { "Dust", sinnersRoad },
                { "Greymoor_17", greymoor },

            };
        }

        public static bool IsExtraProp(string scene, GameObject gameObject)
        {
            if (string.IsNullOrEmpty(scene)) return false;
            if (props == null) Init();

            var sceneProps = props.FirstOrDefault(k => scene.StartsWith(k.Key)).Value;

            if (sceneProps == null) return false;
            //Log.LogError("success");
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer?.sprite == null) return false;
            if (renderer.color != Color.white) return false;

            return sceneProps.Any(p => gameObject.name.StartsWith(p) || renderer.sprite.name.StartsWith(p));
        }
    }
}