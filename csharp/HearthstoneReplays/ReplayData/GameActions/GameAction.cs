﻿#region

using System.Xml.Serialization;

#endregion

namespace HearthstoneReplays.GameActions
{
	[XmlInclude(typeof(TagChange))]
	[XmlInclude(typeof(Action))]
	public abstract class GameAction : GameData
	{
		[XmlAttribute("entity")]
		public int Entity { get; set; }
	}
}