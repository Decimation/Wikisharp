using System.Text;

namespace Wikisharp
{
	public class ReadingListEntry
	{
		public int Id { get; set;}
		public int ListId { get; set;}
		public string Project { get; set;}
		public string Title { get; set;}
		public string Created { get; set;}
		public string Updated { get; set;}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Id: {0}\n", Id);
			sb.AppendFormat("List Id: {0}\n", ListId);
			sb.AppendFormat("Title: {0}\n", Title);
			return sb.ToString();
		}
	}
}