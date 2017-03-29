using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class ShaderGenerator {
  private const string TEMPLATE_FOLDER = "Assets/LeapMotionModules/ElementRenderer/Resources/ShaderTemplates/";
  private const string GENERATION_FOLDER = "Assets/LeapMotionModules/ElementRenderer/Shaders/Generated/";
  private const string TEMPLATE_SUFFIX = ".txt";
  private const string SHADER_SUFFIX = ".shader";

  private const string KEY_SHADER_NAME = "SHADER NAME";
  private const string KEY_PROPERTIES = "PROPERTIES";
  private const string KEY_DEFINES = "DEFINES";
  private const string KEY_SAMPLERS = "SAMPLERS";
  private const string KEY_FRAGMENT_CODE = "FRAGMENT CODE";

  private Dictionary<string, Func<IEnumerable<string>>> _keywordMap = new Dictionary<string, Func<IEnumerable<string>>>();
  private static Regex regex = new Regex(@"\s*<<([\w\s]+)>>");

  public string[] template;

  public List<TextureProperty> textures = new List<TextureProperty>();
  public bool useVertexColors;
  public List<string> defines = new List<string>();

  public ShaderGenerator(string templateName) {
    template = File.ReadAllLines(Path.Combine(TEMPLATE_FOLDER, templateName + TEMPLATE_SUFFIX));
  }

  public Shader GenerateShader(string shaderName) {
    string code = generateShaderCode();
    string path = Path.Combine(GENERATION_FOLDER, shaderName + SHADER_SUFFIX);

    Directory.CreateDirectory(GENERATION_FOLDER);
    File.WriteAllText(path, code);

    AssetDatabase.Refresh();

    return AssetDatabase.LoadAssetAtPath<Shader>(path);
  }

  private string generateShaderCode() {
    StringBuilder builder = new StringBuilder();

    foreach (string line in template) {
      Match match = regex.Match(line);
      if (!match.Success) {
        builder.AppendLine(line);
        continue;
      }

      string keyword = match.Groups[1].Value;

      Func<IEnumerable<string>> generator;
      if (!_keywordMap.TryGetValue(keyword, out generator)) {
        Debug.LogError("Could not find generator for keyword [" + keyword + "]");
        builder.AppendLine(line);
        continue;
      }

      string marker = "<<" + keyword + ">>";
      foreach (var generated in generator()) {
        string newLine = line.Replace(marker, generated);
        builder.AppendLine(newLine);
      }
    }

    return builder.ToString();
  }

  private IEnumerable<string> generateShaderName() {
    yield return "TEST SHADER";
  }

  private IEnumerable<string> generateProperties() {
    foreach (var texture in textures) {
      yield return texture.name + " (\"" + texture.displayName + "\", 2D) = \"white\" {}";
    }
  }

  private IEnumerable<string> generateDefines() {
    foreach (var define in defines) {
      yield return "#define " + define;
    }
  }

  private IEnumerable<string> generateSamplers() {
    foreach (var texture in textures) {
      yield return "sampler2D " + texture.name + ";";
    }
  }

  private IEnumerable<string> generateFragmentCode() {
    string fragmentInit;
    if (useVertexColors) {
      fragmentInit = "fixed4 color = i.color;";
    } else if (textures.Count != 0) {
      var firstTex = textures[0];
      fragmentInit = "fixed4 color = tex2D(" + firstTex.name + ", i.uv" + firstTex.channel.Index() + ");";
    } else {
      fragmentInit = "fixed4 color = fixed4(1,1,1,1);";
    }
    yield return fragmentInit;

    for (int i = useVertexColors ? 0 : 1; i < textures.Count; i++) {
      yield return "color *= tex2D(" + textures[i].name + ", i.uv" + textures[i].channel.Index() + ");";
    }

    yield return "return color;";
  }

  public struct TextureProperty {
    public string name;
    public string displayName;
    public UVChannelFlags channel;
  }
}
