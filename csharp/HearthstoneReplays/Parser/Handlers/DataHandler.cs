﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthstoneReplays.Entities;
using HearthstoneReplays.GameActions;
using HearthstoneReplays.Hearthstone.Enums;
using HearthstoneReplays.ReplayData.Meta;
using Action = HearthstoneReplays.GameActions.Action;

#endregion

namespace HearthstoneReplays.Parser.Handlers
{
	public class DataHandler
	{
		public static void Handle(string timestamp, string data, ParserState state)
		{
			var trimmed = data.Trim();
			var indentLevel = data.Length - trimmed.Length;
			data = trimmed;

			if(state.Node != null && indentLevel <= state.Node.IndentLevel)
				state.Node = state.Node.Parent ?? state.Node;


			if(data == "CREATE_GAME")
			{
				state.CurrentGame = new Game {Data = new List<GameData>()};
				state.Replay.Games.Add(state.CurrentGame);
				state.Node = new Node(typeof(Game), state.CurrentGame, 0, null);
				return;
			}

			var match = Regexes.ActionCreategameRegex.Match(data);
			if(match.Success)
			{
				var id = match.Groups[1].Value;
				Debug.Assert(id == "1");
				var gEntity = new GameEntity {Id = int.Parse(id), Tags = new List<Tag>()};
				state.CurrentGame.Data.Add(gEntity);
				state.Node = new Node(typeof(GameEntity), gEntity, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionCreategamePlayerRegex.Match(data);
			if(match.Success)
			{
				var id = match.Groups[1].Value;
				var playerId = match.Groups[2].Value;
				var accountHi = match.Groups[3].Value;
				var accountLo = match.Groups[4].Value;
				var pEntity = new PlayerEntity
				{
					Id = int.Parse(id),
					AccountHi = accountHi,
					AccountLo = accountLo,
					PlayerId = int.Parse(playerId),
					Tags = new List<Tag>()
				};
				state.CurrentGame.Data.Add(pEntity);
				state.Node = new Node(typeof(PlayerEntity), pEntity, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionStartRegex.Match(data);
			if(match.Success)
			{
				var rawEntity = match.Groups[1].Value;
				var rawType = match.Groups[2].Value;
				var index = match.Groups[3].Value;
				var rawTarget = match.Groups[4].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var target = Helper.ParseEntity(rawTarget, state);
				var type = Helper.ParseEnum<POWER_SUBTYPE>(rawType);
				var action = new Action
				{
					Data = new List<GameData>(),
					Entity = entity,
					Index = int.Parse(index),
					Target = target,
					//TimeStamp = timestamp,
					Type = type
				};
				if(state.Node.Type == typeof(Game))
					((Game)state.Node.Object).Data.Add(action);
				else if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(action);
				else
					throw new Exception("Invalid node " + state.Node.Type);
				state.Node = new Node(typeof(Action), action, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionMetadataRegex.Match(data);
			if(match.Success)
			{
				var rawMeta = match.Groups[1].Value;
				var rawData = match.Groups[2].Value;
				var info = match.Groups[3].Value;
				var parsedData = Helper.ParseEntity(rawData, state);
				var meta = Helper.ParseEnum<METADATA_TYPE>(rawMeta);
				var metaData = new MetaData {Data = parsedData, Info = info, Meta = meta, MetaInfo = new List<Info>()};
				if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(metaData);
				else
					throw new Exception("Invalid node " + state.Node.Type);
				state.Node = new Node(typeof(MetaData), metaData, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionMetaDataInfoRegex.Match(data);
			if(match.Success)
			{
				var index = match.Groups[1].Value;
				var rawEntity = match.Groups[2].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var metaInfo = new Info {Id = entity, Index = index};
				if(state.Node.Type == typeof(MetaData))
					((MetaData)state.Node.Object).MetaInfo.Add(metaInfo);
				else
					throw new Exception("Invalid node " + state.Node.Type);
			}

			match = Regexes.ActionShowEntityRegex.Match(data);
			if(match.Success)
			{
				var rawEntity = match.Groups[1].Value;
				var cardId = match.Groups[2].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var showEntity = new ShowEntity {CardId = cardId, Entity = entity, Tags = new List<Tag>()};
				if(state.Node.Type == typeof(Game))
					((Game)state.Node.Object).Data.Add(showEntity);
				else if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(showEntity);
				else
					throw new Exception("Invalid node " + state.Node.Type);
				state.Node = new Node(typeof(ShowEntity), showEntity, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionHideEntityRegex.Match(data);
			if(match.Success)
			{
				var rawEntity = match.Groups[1].Value;
				var tagName = match.Groups[2].Value;
				var value = match.Groups[3].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var tag = Helper.ParseTag(tagName, value);
				var hideEntity = new HideEntity {Entity = entity, TagName = tag.Name, TagValue = tag.Value};
				if(state.Node.Type == typeof(Game))
					((Game)state.Node.Object).Data.Add(hideEntity);
				else if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(hideEntity);
				else
					throw new Exception("Invalid node: " + state.Node.Type);
				return;
			}

			match = Regexes.ActionFullentityRegex1.Match(data);
			if(!match.Success)
				match = Regexes.ActionFullentityRegex2.Match(data);
			if(match.Success)
			{
				var rawEntity = match.Groups[1].Value;
				var cardId = match.Groups[2].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var showEntity = new FullEntity {CardId = cardId, Id = entity, Tags = new List<Tag>()};
				if(state.Node.Type == typeof(Game))
					((Game)state.Node.Object).Data.Add(showEntity);
				else if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(showEntity);
				else
					throw new Exception("Invalid node " + state.Node.Type);
				state.Node = new Node(typeof(FullEntity), showEntity, indentLevel, state.Node);
				return;
			}

			match = Regexes.ActionTagChangeRegex.Match(data);
			if(match.Success)
			{
				var rawEntity = match.Groups[1].Value;
				var tagName = match.Groups[2].Value;
				var value = match.Groups[3].Value;
				var entity = Helper.ParseEntity(rawEntity, state);
				var tag = Helper.ParseTag(tagName, value);
				if(tag.Name == (int)GAME_TAG.ENTITY_ID)
                {
                    int tmp;
                    if(!int.TryParse(rawEntity, out tmp) && !rawEntity.StartsWith("[") && rawEntity != "GameEntity")
                    {
                        if (entity != tag.Value)
                        {
                            entity = tag.Value;
                            var tmpName = ((PlayerEntity) state.CurrentGame.Data[1]).Name;
                            ((PlayerEntity) state.CurrentGame.Data[1]).Name =
                                ((PlayerEntity) state.CurrentGame.Data[2]).Name;
                            ((PlayerEntity) state.CurrentGame.Data[2]).Name = tmpName;
                            foreach (var dataObj in ((Game) state.Node.Object).Data)
                            {
                                var tChange = dataObj as TagChange;
                                if (tChange != null)
                                    tChange.Entity = tChange.Entity == 2 ? 3 : 2;
                            }
                        }
                    }
                }
				var tagChange = new TagChange {Entity = entity, Name = tag.Name, Value = tag.Value};
				if(state.Node.Type == typeof(Game))
					((Game)state.Node.Object).Data.Add(tagChange);
				else if(state.Node.Type == typeof(Action))
					((Action)state.Node.Object).Data.Add(tagChange);
				else
					throw new Exception("Invalid node " + state.Node.Type);
				return;
			}

			match = Regexes.ActionTagRegex.Match(data);
			if(match.Success)
			{
				var tagName = match.Groups[1].Value;
				var value = match.Groups[2].Value;
				var tag = Helper.ParseTag(tagName, value);
				if(tag.Name == (int)GAME_TAG.CURRENT_PLAYER)
					state.FirstPlayerId = ((PlayerEntity)state.Node.Object).Id;
				if(state.Node.Type == typeof(GameEntity))
					((GameEntity)state.Node.Object).Tags.Add(tag);
				else if(state.Node.Type == typeof(PlayerEntity))
					((PlayerEntity)state.Node.Object).Tags.Add(tag);
				else if(state.Node.Type == typeof(FullEntity))
					((FullEntity)state.Node.Object).Tags.Add(tag);
				else if(state.Node.Type == typeof(ShowEntity))
					((ShowEntity)state.Node.Object).Tags.Add(tag);
				else
					throw new Exception("Invalid node " + state.Node.Type + " -- " + data);
			}
		}
	}
}