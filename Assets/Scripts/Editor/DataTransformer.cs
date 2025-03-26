using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Assets.Scripts.Data;
using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR
	[MenuItem("Tools/ParseExcel %#K")]
	public static void ParseExcelDataToJson()
	{
		ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
		ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
		// ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
		// ParseExcelDataToJson<ProjectileDataLoader, ProjectileData>("Projectile");
		// ParseExcelDataToJson<EnvDataLoader, EnvData>("Env");
		// ParseExcelDataToJson<EffectDataLoader, EffectData>("Effect");
		// ParseExcelDataToJson<AoEDataLoader, AoEData>("AoE");

		Debug.Log("DataTransformer Completed");
	}

	#region Helpers
	private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : new()
	{
		Loader loader = new Loader();
		FieldInfo field = loader.GetType().GetFields()[0];
		field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename));

		string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
		File.WriteAllText($"{Application.dataPath}/@Resources/04_Data/JsonData/{filename}Data.json", jsonStr);
		AssetDatabase.Refresh();
	}

	private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename) where LoaderData : new()
	{
		List<LoaderData> loaderDatas = new List<LoaderData>();

		string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/04_Data/ExcelData/{filename}Data.csv").Split("\n");

		for (int l = 1; l < lines.Length; l++)
		{
			string[] row = lines[l].Replace("\r", "").Split(',');
			if (row.Length == 0)
				continue;
			if (string.IsNullOrEmpty(row[0]))
				continue;

			LoaderData loaderData = new LoaderData();
			var fields = GetFieldsInBase(typeof(LoaderData));

			for (int f = 0; f < fields.Count; f++)
			{
				FieldInfo field = loaderData.GetType().GetField(fields[f].Name);
				Type type = field.FieldType;

				if (field.HasAttribute(typeof(NonSerializedAttribute)))
					continue;

				if (type.IsGenericType)
				{
					object value = ConvertList(row[f], type);
					field.SetValue(loaderData, value);
				}
				else
				{
					object value = ConvertValue(row[f], field.FieldType);
					field.SetValue(loaderData, value);
				}
			}

			loaderDatas.Add(loaderData);
		}

		return loaderDatas;
	}

	// private static object ConvertValue(string value, Type type)
	// {
	// 	if (string.IsNullOrEmpty(value))
	// 		return null;

	// 	TypeConverter converter = TypeDescriptor.GetConverter(type);
	// 	return converter.ConvertFromString(value);
	// }

	private static object ConvertValue(string value, Type type)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		// 문자열 앞뒤 공백 및 따옴표 제거
		value = value.Trim();
		value = value.Trim('"', '\'');
		
		// Int32 타입일 경우 안전한 변환 시도
		if (type == typeof(int))
		{
			if (int.TryParse(value, out int result))
				return result;
			
			Debug.LogWarning($"값 '{value}'을(를) int로 변환할 수 없습니다. 기본값 0을 사용합니다.");
			return 0;
		}
		
		// Bool 타입일 경우 특별 처리
		if (type == typeof(bool))
		{
			// 대소문자 구분 없이 비교
			if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				return true;
			if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
				return false;
			
			// "TRUE"/"FALSE" 형식 처리
			if (value.Equals("TRUE", StringComparison.Ordinal))
				return true;
			if (value.Equals("FALSE", StringComparison.Ordinal))
				return false;
			
			// 1/0 형식 처리
			if (value == "1")
				return true;
			if (value == "0")
				return false;
			
			Debug.LogWarning($"값 '{value}'을(를) bool로 변환할 수 없습니다. 기본값 false를 사용합니다.");
			return false;
		}
		
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(type);
			return converter.ConvertFromString(value);
		}
		catch (Exception ex)
		{
			Debug.LogError($"값 '{value}'을(를) {type.Name} 타입으로 변환하는 중 오류 발생: {ex.Message}");
			
			// 기본값 반환
			if (type == typeof(string)) return "";
			if (type == typeof(int)) return 0;
			if (type == typeof(float)) return 0f;
			if (type == typeof(bool)) return false;
			
			return null;
		}
	}
	private static object ConvertList(string value, Type type)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		// Reflection
		Type valueType = type.GetGenericArguments()[0];
		Type genericListType = typeof(List<>).MakeGenericType(valueType);
		var genericList = Activator.CreateInstance(genericListType) as IList;

		// Parse Excel
		var list = value.Split('&').Select(x => ConvertValue(x, valueType)).ToList();

		foreach (var item in list)
			genericList.Add(item);

		return genericList;
	}

	public static List<FieldInfo> GetFieldsInBase(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
	{
		List<FieldInfo> fields = new List<FieldInfo>();
		HashSet<string> fieldNames = new HashSet<string>(); // 중복방지
		Stack<Type> stack = new Stack<Type>();

		while (type != typeof(object))
		{
			stack.Push(type);
			type = type.BaseType;
		}

		while (stack.Count > 0)
		{
			Type currentType = stack.Pop();

			foreach (var field in currentType.GetFields(bindingFlags))
			{
				if (fieldNames.Add(field.Name))
				{
					fields.Add(field);
				}
			}
		}

		return fields;
	}
	#endregion

#endif
}