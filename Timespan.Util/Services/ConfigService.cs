namespace Timespan.Util.Services;

using System;
using System.Collections.Generic;
using System.IO;


public class ConfigService {

	private string bufferedFile;
	private string _path;
	internal string[] lines;
	private Dictionary<string, Configuration> configurationBuffer;

	public ConfigService(string path) {
		_path = path;
		LoadConfigurations();
	}

	private bool IsDecoratorLine(string line) {
		bool containsDecorator = false;
		for (int i = 0; i < line.Length; i++) {
			if(line[i] == '#') {
				containsDecorator = true;
				continue;
			}
			if (line[i] != '\n')
				return false;
		}
		return containsDecorator;
	}

	public Configuration GenreateConfiguration(string configurationName, bool alwaysAppend) {
//        if(!alwaysAppend && configurationBuffer.ContainsKey(configurationName)) {
//			return GetConfiguratioinByName(configurationName, false);
//		}
//		//buffer the whole text of the config into a string
//		string buffer = "";
//		//foreach(string line in lines) { 
//		//	buffer += line
//		//}

//		//if the buffer isn't empty and doesn't have a newline at its end, add a newline

//		if not buffer.endswith('\n') and not buffer == '':
//            buffer += '\n'
//		//if the buffer isn't still empty, add two additional newlines
//		if not buffer == '':
//            buffer += '\n\n'
//		//add a row of '#'
//		for i in range(CONFIGURATION_HEADER_LENGTH):

//			buffer += '#'

//		buffer += '\n'
////calculate the amount of '#' that must be added to the configuration's name to make it 60 characters long
////add half the '#' and a space to the left of the configuration's name
//		for i in range(math.floor((CONFIGURATION_HEADER_LENGTH - 2 - len(configuration_name)) / 2)):

//			buffer += '#'

//		buffer += ' '

//		buffer += configuration_name

//		buffer += ' '
////add a space and the rest of the '#' to the right of the configuration's name
//		for i in range(math.ceil((CONFIGURATION_HEADER_LENGTH - 2 - len(configuration_name)) / 2)):

//			buffer += '#'

//		buffer += '\n'
////add a row of '#'
//		for i in range(CONFIGURATION_HEADER_LENGTH):

//			buffer += '#'

//		buffer += '\n'
////write the buffer to the config file
//		self.overwrite_config_file(buffer)

//		return self.get_configuration_by_name(configuration_name)
		return null;
	}

	public Configuration? GetConfiguratioinByName(string configurationName, bool generateIfExists) {
		Configuration? result;
		if(!configurationBuffer.TryGetValue(configurationName, out result) && generateIfExists)
			GenreateConfiguration(configurationName, false);
		return result;
    }

	private void LoadConfigurations() {

		for(int lineIndex=0; lineIndex<lines.Length; lineIndex++) {
			if (! lines[lineIndex].StartsWith('#'))
				continue;
			int nameStartIndex = 0;
			while (nameStartIndex < lines[lineIndex].Length) { 
				if(lines[lineIndex][nameStartIndex] == '#') {
					nameStartIndex++;
					continue;
				}
                nameStartIndex++;
				break;
            }
			int nameFinishIndex = lines[lineIndex].Length-1;
			while (nameFinishIndex > 0) { 
				if(lines[lineIndex][nameFinishIndex] == '#') {
                    nameFinishIndex--;
					continue;
				}
                nameFinishIndex--;
				break;
			}
            string configurationName = lines[lineIndex].Substring(nameStartIndex, nameFinishIndex - nameStartIndex);

		} 
	}

	private string[] LoadFile() {
		string fileContent = "";
		using (FileStream stream = File.Open(_path, FileMode.OpenOrCreate))
			using (StreamReader reader = new StreamReader(stream))
				fileContent = reader.ReadToEnd();
		return fileContent.Split('\n');
	}

}

public class Configuration(ConfigService _manager, string _name, int _offset) {

	private ConfigService manager = _manager;
	private string name = _name;
	private int offset = _offset;


}