using System;
using System.Text;

namespace Wikisharp
{
	public class ReadingList
	{
		public int      Id          { get; set; }
		public string   Name        { get; set; }
		public string   Description { get; set; }
		public string Created     { get; set; }
		public string Updated     { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Id: {0}\n", Id);
			sb.AppendFormat("Name: {0}\n", Name);
			return sb.ToString();
		}
	}
}