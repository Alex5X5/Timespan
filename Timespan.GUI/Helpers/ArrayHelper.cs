namespace Timespan.GUI.Helpers;

public static class ArrayHelper {

	public static T[,] ResizeArray<T>(T[,]? original, int rows, int cols, T fallback) {
		return ResizeArray(original, rows, cols, (r, c) => fallback);
	}

	public static T[,] ResizeArray<T>(T[,]? original, int rows, int cols, Func<int, int, T> fallback) {
		original ??= new T[0, 0];
		var newArray = new T[rows, cols];
		int minRows = Math.Min(rows, original.GetLength(0));
		int minCols = Math.Min(cols, original.GetLength(1));
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				if (i < minRows && j < minCols) {
					newArray[i, j] = original[i, j];
				} else {
					newArray[i, j] = fallback(i, j);
				}
			}
		}
		return newArray;
	}
}
