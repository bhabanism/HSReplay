﻿#region

using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace HearthstoneReplays.ReplayData.Meta
{
	public class SendChoices : GameData
	{
		[XmlElement("Chioce", typeof(Choice))]
		public List<Choice> Choices;

		[XmlAttribute("entity")]
		public string Entity { get; set; }

		[XmlAttribute("type")]
		public string Type { get; set; }
	}
}