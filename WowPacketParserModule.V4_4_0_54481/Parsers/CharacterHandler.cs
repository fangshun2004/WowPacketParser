﻿using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V4_4_0_54481.Parsers
{
    public static class CharacterHandler
    {
        public static PlayerGuidLookupData ReadPlayerGuidLookupData(Packet packet, params object[] idx)
        {
            PlayerGuidLookupData data = new PlayerGuidLookupData();

            packet.ResetBitReader();
            packet.ReadBit("IsDeleted", idx);
            var bits15 = (int)packet.ReadBits(6);

            var count = new int[5];
            for (var i = 0; i < 5; ++i)
                count[i] = (int)packet.ReadBits(7);

            for (var i = 0; i < 5; ++i)
                packet.ReadWoWString("Name Declined", count[i], i, idx);

            packet.ReadPackedGuid128("AccountID", idx);
            packet.ReadPackedGuid128("BnetAccountID", idx);
            packet.ReadPackedGuid128("Player Guid", idx);

            packet.ReadUInt64("GuildClubMemberID", idx);
            packet.ReadUInt32("VirtualRealmAddress", idx);

            data.Race = packet.ReadByteE<Race>("Race", idx);
            data.Gender = packet.ReadByteE<Gender>("Gender", idx);
            data.Class = packet.ReadByteE<Class>("Class", idx);
            data.Level = packet.ReadByte("Level", idx);
            packet.ReadByte("Unused915", idx);

            data.Name = packet.ReadWoWString("Name", bits15, idx);

            return data;
        }

        public static void ReadUnlockedConditionalAppearance(Packet packet, params object[] indexes)
        {
            packet.ReadUInt32("AchievementId", indexes);
            packet.ReadUInt32("Unused", indexes);
        }

        public static void ReadRaceUnlockData(Packet packet, params object[] idx)
        {
            packet.ReadInt32E<Race>("RaceID", idx);
            packet.ResetBitReader();
            packet.ReadBit("HasExpansion", idx);
            packet.ReadBit("HasAchievement", idx);
            packet.ReadBit("HasHeritageArmor", idx);
            packet.ReadBit("IsLocked", idx);
        }

        public static void ReadRaceLimitDisableInfo(Packet packet, params object[] idx)
        {
            packet.ReadInt32E<Race>("RaceID", idx);
            packet.ReadInt32("BlockReason", idx);
        }

        public static void ReadChrCustomizationChoice(Packet packet, params object[] indexes)
        {
            packet.ReadUInt32("ChrCustomizationOptionID", indexes);
            packet.ReadUInt32("ChrCustomizationChoiceID", indexes);
        }

        public static void ReadVisualItemInfo(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("DisplayID", idx);
            packet.ReadUInt32("DisplayEnchantID", idx);
            packet.ReadInt32("SecondaryItemModifiedAppearanceID", idx);
            packet.ReadByteE<InventoryType>("InvType", idx);
            packet.ReadByte("Subclass", idx);
        }

        public static void ReadCustomTabardInfo(Packet packet, params object[] idx)
        {
            packet.ReadInt32("EmblemStyle", idx);
            packet.ReadInt32("EmblemColor", idx);
            packet.ReadInt32("BorderStyle", idx);
            packet.ReadInt32("BorderColor", idx);
            packet.ReadInt32("BackgroundColor", idx);
        }

        public static void ReadCharactersListEntry(Packet packet, params object[] idx)
        {
            var playerGuid = packet.ReadPackedGuid128("Guid", idx);
            packet.ReadUInt64("GuildClubMemberID", idx);
            packet.ReadByte("ListPosition", idx);
            var race = packet.ReadByteE<Race>("RaceID", idx);
            var @class = packet.ReadByteE<Class>("ClassID", idx);
            packet.ReadByteE<Gender>("SexID", idx);
            var customizationCount = packet.ReadUInt32();
            var level = packet.ReadByte("ExperienceLevel", idx);
            var zone = packet.ReadInt32<ZoneId>("ZoneID", idx);
            var mapId = packet.ReadInt32<MapId>("MapID", idx);
            var pos = packet.ReadVector3("PreloadPos", idx);
            packet.ReadPackedGuid128("GuildGUID", idx);
            packet.ReadUInt32("Flags", idx);
            packet.ReadUInt32("Flags2", idx);
            packet.ReadUInt32("Flags3", idx);
            packet.ReadUInt32("PetCreatureDisplayID", idx);
            packet.ReadUInt32("PetExperienceLevel", idx);
            packet.ReadUInt32("PetCreatureFamilyID", idx);

            for (uint j = 0; j < 2; ++j)
                packet.ReadInt32("ProfessionIDs", idx, j);

            for (uint j = 0; j < 34; ++j)
                ReadVisualItemInfo(packet, idx, j);

            packet.ReadTime64("LastPlayedTime", idx);
            packet.ReadInt16("SpecID", idx);
            packet.ReadInt32("Unknown703", idx);
            packet.ReadInt32("LastLoginVersion", idx);
            packet.ReadUInt32("Flags4", idx);
            var mailSenderLengths = new uint[packet.ReadUInt32()];
            var mailSenderTypes = new uint[packet.ReadUInt32()];
            packet.ReadUInt32("OverrideSelectScreenFileDataID", idx);

            ReadCustomTabardInfo(packet, idx, "PersonalTabard");

            for (var j = 0u; j < customizationCount; ++j)
                ReadChrCustomizationChoice(packet, idx, "Customizations", j);

            for (var j = 0; j < mailSenderTypes.Length; ++j)
                packet.ReadUInt32("MailSenderType", idx, j);

            packet.ResetBitReader();

            var nameLength = packet.ReadBits("Character Name Length", 6, idx);
            var firstLogin = packet.ReadBit("FirstLogin", idx);
            packet.ReadBit("BoostInProgress", idx);
            packet.ReadBits("UnkWod61x", 5, idx);
            packet.ReadBits("Unk440_1", 2, idx);
            packet.ReadBit("RpeResetAvailable", idx);
            packet.ReadBit("RpeResetQuestClearAvailable", idx);

            for (var j = 0; j < mailSenderLengths.Length; ++j)
                mailSenderLengths[j] = packet.ReadBits(6);

            for (var j = 0; j < mailSenderLengths.Length; ++j)
                if (mailSenderLengths[j] > 1)
                    packet.ReadDynamicString("MailSender", mailSenderLengths[j], idx);

            var name = packet.ReadWoWString("Character Name", nameLength, idx);

            if (firstLogin)
            {
                PlayerCreateInfo startPos = new PlayerCreateInfo { Race = race, Class = @class, Map = (uint)mapId, Zone = (uint)zone, Position = pos, Orientation = 0 };
                Storage.StartPositions.Add(startPos, packet.TimeSpan);
            }

            var playerInfo = new Player { Race = race, Class = @class, Name = name, FirstLogin = firstLogin, Level = level, Type = ObjectType.Player };
            if (Storage.Objects.ContainsKey(playerGuid))
                Storage.Objects[playerGuid] = new Tuple<WoWObject, TimeSpan?>(playerInfo, packet.TimeSpan);
            else
                Storage.Objects.Add(playerGuid, playerInfo, packet.TimeSpan);
        }

        public static void ReadAzeriteEssenceData(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("Index", idx);
            packet.ReadUInt32("AzeriteEssenceID", idx);
            packet.ReadUInt32("Rank", idx);
            packet.ReadBit("SlotUnlocked", idx);
            packet.ResetBitReader();
        }

        public static void ReadInspectItemData(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("CreatorGUID", idx);
            packet.ReadByte("Index", idx);

            var azeritePowerCount = packet.ReadUInt32("AzeritePowersCount", idx);
            var azeriteEssenceCount = packet.ReadUInt32("AzeriteEssenceCount", idx);

            for (int j = 0; j < azeritePowerCount; j++)
                packet.ReadInt32("AzeritePowerId", idx, j);

            Substructures.ItemHandler.ReadItemInstance(packet, idx);

            packet.ReadBit("Usable", idx);
            var enchantsCount = packet.ReadBits("EnchantsCount", 4, idx);
            var gemsCount = packet.ReadBits("GemsCount", 2, idx);
            packet.ResetBitReader();

            for (int i = 0; i < azeriteEssenceCount; i++)
                ReadAzeriteEssenceData(packet, "AzeriteEssence", i);

            for (int i = 0; i < enchantsCount; i++)
            {
                packet.ReadUInt32("Id", idx, i);
                packet.ReadByte("Index", idx, i);
            }

            for (int i = 0; i < gemsCount; i++)
            {
                packet.ReadByte("Slot", idx, i);
                Substructures.ItemHandler.ReadItemInstance(packet, idx, i);
            }
        }

        public static void ReadPlayerModelDisplayInfo(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("InspecteeGUID", idx);
            packet.ReadInt32("SpecializationID", idx);
            var itemCount = packet.ReadUInt32();
            var nameLen = packet.ReadBits(6);
            packet.ResetBitReader();
            packet.ReadByteE<Gender>("GenderID", idx);
            packet.ReadByteE<Race>("Race", idx);
            packet.ReadByteE<Class>("ClassID", idx);
            var customizationCount = packet.ReadUInt32();
            packet.ReadWoWString("Name", nameLen, idx);

            for (var j = 0u; j < customizationCount; ++j)
                ReadChrCustomizationChoice(packet, idx, "Customizations", j);

            for (int i = 0; i < itemCount; i++)
                ReadInspectItemData(packet, idx, i);
        }

        public static void ReadPVPBracketData(Packet packet, params object[] idx)
        {
            packet.ReadByte("Bracket", idx);
            packet.ReadInt32("Unused3", idx);
            packet.ReadInt32("Rating", idx);
            packet.ReadInt32("Rank", idx);
            packet.ReadInt32("WeeklyPlayed", idx);
            packet.ReadInt32("WeeklyWon", idx);
            packet.ReadInt32("SeasonPlayed", idx);
            packet.ReadInt32("SeasonWon", idx);
            packet.ReadInt32("WeeklyBestRating", idx);
            packet.ReadInt32("SeasonBestRating", idx);
            packet.ReadInt32("PvpTierID", idx);
            packet.ReadInt32("WeeklyBestWinPvpTierID", idx);
            packet.ReadInt32("Unused1", idx);
            packet.ReadInt32("Unused2", idx);
            packet.ReadInt32("RoundsSeasonPlayed", idx);
            packet.ReadInt32("RoundsSeasonWon", idx);
            packet.ReadInt32("RoundsWeeklyPlayed", idx);
            packet.ReadInt32("RoundsWeeklyWon", idx);

            packet.ResetBitReader();
            packet.ReadBit("Disqualified", idx);
        }

        [Parser(Opcode.SMSG_BARBER_SHOP_RESULT)]
        public static void HandleBarberShopResult(Packet packet)
        {
            packet.ReadInt32E<BarberShopResult>("Result");
            packet.ReadBit("IgnoreChair");
        }

        [Parser(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
        public static void HandleEnumCharactersResult(Packet packet)
        {
            packet.ReadBit("Success");
            packet.ReadBit("IsDeletedCharacters");
            packet.ReadBit("IsNewPlayerRestrictionSkipped");
            packet.ReadBit("IsNewPlayerRestricted");
            packet.ReadBit("IsNewPlayer");
            packet.ReadBit("IsTrialAccountRestricted");
            var hasDisabledClassesMask = packet.ReadBit("HasDisabledClassesMask");

            var charsCount = packet.ReadUInt32("CharactersCount");
            packet.ReadInt32("MaxCharacterLevel");
            var raceUnlockCount = packet.ReadUInt32("RaceUnlockCount");
            var unlockedConditionalAppearanceCount = packet.ReadUInt32("UnlockedConditionalAppearanceCount");
            var raceLimitDisablesCount = packet.ReadUInt32("RaceLimitDisablesCount");

            if (hasDisabledClassesMask)
                packet.ReadUInt32("DisabledClassesMask");

            for (var i = 0u; i < unlockedConditionalAppearanceCount; ++i)
                ReadUnlockedConditionalAppearance(packet, "UnlockedConditionalAppearances", i);

            for (var i = 0u; i < raceLimitDisablesCount; i++)
                ReadRaceLimitDisableInfo(packet, "RaceLimitDisableInfo", i);

            for (var i = 0u; i < charsCount; ++i)
                ReadCharactersListEntry(packet, i, "Characters");

            for (var i = 0u; i < raceUnlockCount; ++i)
                ReadRaceUnlockData(packet, i, "RaceUnlockData");
        }

        [Parser(Opcode.SMSG_UNDELETE_COOLDOWN_STATUS_RESPONSE)]
        public static void HandleUndeleteCooldownStatusResponse(Packet packet)
        {
            packet.ReadBit("OnCooldown");
            packet.ReadUInt32("MaxCooldown"); // In Sec
            packet.ReadUInt32("CurrentCooldown"); // In Sec
        }

        [Parser(Opcode.CMSG_CHAR_DELETE)]
        public static void HandleClientCharDelete(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGUID");
        }

        [Parser(Opcode.SMSG_DELETE_CHAR)]
        public static void HandleDeleteChar(Packet packet)
        {
            packet.ReadByteE<ResponseCode>("Response");
        }

        [Parser(Opcode.SMSG_CREATE_CHAR)]
        public static void HandleCreateChar(Packet packet)
        {
            packet.ReadByteE<ResponseCode>("Response");
            packet.ReadPackedGuid128("GUID");
        }

        [Parser(Opcode.CMSG_ENUM_CHARACTERS)]
        [Parser(Opcode.CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS)]
        public static void HandleCharacterNull(Packet packet)
        {
        }

        [Parser(Opcode.SMSG_LEVEL_UP_INFO)]
        public static void HandleLevelUpInfo(Packet packet)
        {
            packet.ReadInt32("Level");
            packet.ReadInt32("HealthDelta");

            for (var i = 0; i < 10; i++)
                packet.ReadInt32("PowerDelta", (PowerType)i);

            for (var i = 0; i < 5; i++)
                packet.ReadInt32("StatDelta", (StatType)i);

            packet.ReadInt32("NumNewTalents");
            packet.ReadInt32("NumNewPvpTalentSlots");
        }

        [Parser(Opcode.CMSG_REQUEST_PLAYED_TIME)]
        public static void HandleClientPlayedTime(Packet packet)
        {
            packet.ReadBit("TriggerScriptEvent");
        }

        [Parser(Opcode.SMSG_PLAYED_TIME)]
        public static void HandleServerPlayedTime(Packet packet)
        {
            packet.ReadInt32("TotalTime");
            packet.ReadInt32("LevelTime");

            packet.ReadBit("TriggerEvent");
        }

        [Parser(Opcode.SMSG_STAND_STATE_UPDATE)]
        public static void HandleStandStateUpdate(Packet packet)
        {
            packet.ReadInt32("AnimKitID");
            packet.ReadByteE<StandState>("State");
        }

        [Parser(Opcode.CMSG_STAND_STATE_CHANGE)]
        public static void HandleStandStateChange(Packet packet)
        {
            packet.ReadInt32E<StandState>("StandState");
        }

        [Parser(Opcode.SMSG_CHARACTER_RENAME_RESULT)]
        public static void HandleServerCharRename(Packet packet)
        {
            packet.ReadByte("Result");

            packet.ResetBitReader();
            var hasGuid = packet.ReadBit("HasGuid");
            var nameLength = packet.ReadBits(6);

            if (hasGuid)
                packet.ReadPackedGuid128("Guid");

            packet.ReadWoWString("Name", nameLength);
        }

        [Parser(Opcode.SMSG_CHAR_CUSTOMIZE_FAILURE)]
        public static void HandleServerCharCustomizeResult(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("Guid");
        }

        [Parser(Opcode.SMSG_CHAR_CUSTOMIZE_SUCCESS)]
        public static void HandleServerCharCustomize(Packet packet)
        {
            packet.ReadPackedGuid128("CharGUID");
            packet.ReadByte("SexID");

            var customizationCount = packet.ReadUInt32();
            for (var j = 0u; j < customizationCount; ++j)
                ReadChrCustomizationChoice(packet, "Customizations", j);

            packet.ResetBitReader();
            var bits55 = packet.ReadBits(6);
            packet.ReadWoWString("Name", bits55);
        }

        [Parser(Opcode.SMSG_CHAR_FACTION_CHANGE_RESULT)]
        public static void HandleCharFactionChangeResult(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("Guid");

            packet.ResetBitReader();

            var bit72 = packet.ReadBit("HasDisplayInfo");
            if (bit72)
            {
                packet.ResetBitReader();
                var nameLength = packet.ReadBits(6);

                packet.ReadByte("SexID");
                packet.ReadByte("RaceID");
                var customizationCount = packet.ReadUInt32();
                packet.ReadWoWString("Name", nameLength);

                for (var j = 0u; j < customizationCount; ++j)
                    ReadChrCustomizationChoice(packet, "Customizations", j);
            }
        }

        [Parser(Opcode.SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT)]
        public static void HandleGenerateRandomCharacterNameResponse(Packet packet)
        {
            packet.ReadBit("Success");
            var nameLength = packet.ReadBits(6);

            packet.ReadWoWString("Name", nameLength);
        }

        [Parser(Opcode.SMSG_HEALTH_UPDATE)]
        public static void HandleHealthUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadInt64("Health");
        }

        [Parser(Opcode.SMSG_INSPECT_RESULT)]
        public static void HandleInspectResult(Packet packet)
        {
            ReadPlayerModelDisplayInfo(packet, "DisplayInfo");
            var pvpTalentCount = packet.ReadUInt32("PvpTalentsCount");
            packet.ReadInt32("ItemLevel");
            packet.ReadByte("LifetimeMaxRank");
            packet.ReadUInt16("TodayHK");
            packet.ReadUInt16("YesterdayHK");
            packet.ReadUInt32("LifetimeHK");
            packet.ReadUInt32("HonorLevel");

            for (int i = 0; i < pvpTalentCount; i++)
                packet.ReadUInt16("PvpTalents", i);

            SpellHandler.ReadTalentInfoUpdate(packet, "TalentInfo");

            packet.ResetBitReader();
            var hasGuildData = packet.ReadBit("HasGuildData");
            var hasAzeriteLevel = packet.ReadBit("HasAzeriteLevel");

            for (int i = 0; i < 9; i++)
                ReadPVPBracketData(packet, i, "PVPBracketData");

            if (hasGuildData)
            {
                packet.ReadPackedGuid128("GuildGUID");
                packet.ReadInt32("NumGuildMembers");
                packet.ReadInt32("GuildAchievementPoints");
            }
            if (hasAzeriteLevel)
                packet.ReadInt32("AzeriteLevel");

            packet.ReadInt32("Level", "TraitInspectData");
            packet.ReadInt32("ChrSpecializationID", "TraitInspectData");
            TraitHandler.ReadTraitConfig(packet, "TraitInspectData", "Traits");
        }

        [Parser(Opcode.SMSG_LOG_XP_GAIN)]
        public static void HandleLogXPGain(Packet packet)
        {
            packet.ReadPackedGuid128("Victim");
            packet.ReadInt32("Original");
            packet.ReadByte("Reason");
            packet.ReadInt32("Amount");
            packet.ReadSingle("GroupBonus");
        }
    }
}
