namespace BulkComments {

public class StudentComment {

public StudentCSVRow Row {get; set;}
public string CommentField {get; set;}
public string Comment 
	{ 
	get { 
		if(Row == null || CommentField == null) return null; 
		return Row.Data[CommentField]; 
		}
	}
}
}
