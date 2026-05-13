namespace Timespan.GUI.Services; 

internal class RedirectionService {


	
	internal static void GetRedirectionAnchor<T>() {
		
	}

	internal static void GetRedirectionAnchor<T>(string key) {
		
	}
}


internal class RedirectionAnchor<T> {

	public T currentModel { get; set; }

	public delegate string ModelChanged();
}