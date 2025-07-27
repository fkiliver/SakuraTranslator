<div align="center">
<h1>
  SakuraTranslator
</h1>
</div>

# 介绍
这是一个基于XUnity.AutoTranslator和Sakura模型的Unity游戏本地翻译器，能够提供高质量离线日文翻译  
建议使用[GalTransl-V3翻译模型](https://huggingface.co/SakuraLLM/Sakura-GalTransl-7B-v3)，当前支持版本为Sakura v0.9/v0.10/v1.0，GalTransl v2.6/v3

## TODO
- [ ] 添加历史上文（搁置，难以将对话文本与ui文本区分，需要更好的规则）
- [x] 添加退化检测
- [x] 重新整理对不同模型的支持

## 详细部署流程
如果你不了解AutoTraslator，请从头阅读部署教程：详见[本仓库wiki](https://github.com/fkiliver/SakuraTranslator/wiki)  


# 快速开始

## 启动Sakura服务
此处以本地部署为例  
确保Sakura服务器成功启动并监听`http://127.0.0.1:8080` (端口根据你的启动方式而定，但要配置文件与之保持一致)

## 修改配置文件
### 安装SakuraTranslator
从 [Releases](https://github.com/fkiliver/SakuraTranslator/releases) 下载`SakuraTranslate.dll`放置在Translators文件夹内
 - 如果你使用ReiPatcher，你应该放在`{游戏目录}\{游戏名}_Data\Managed\Translators`
 - 如果你使用BepInEx，你应该放在`{游戏目录}\BepInEx\plugins\XUnity.AutoTranslator\Translators`

### 配置SakuraTranslator
启动一次游戏，这时应当会自动生成配置文件  
修改AutoTranslator的配置文件  
 - 如果你使用ReiPatcher，配置文件应该在`{游戏目录}\AutoTranslator\Config.ini`
 - 如果你使用BepInEx，配置文件应该在`{游戏目录}\BepInEx\config\AutoTranslatorConfig.ini`

首先修改配置文件前两个字段为
```
[Service]
Endpoint=SakuraTranslate
FallbackEndpoint=

[General]
Language=zh
FromLanguage=ja
```
如果你在使用中出现了缺字、方块字等情况，请指定外部字体：
```
[Behaviour]
OverrideFont= ##填写你系统中已安装的字体名
OverrideFontTextMeshPro=arialuni_sdf_u2018 ##或arialuni_sdf_u2019
```
其中arialuni_sdf_u201x可以从[字体文件](https://github.com/bbepis/XUnity.AutoTranslator/releases/download/v5.4.5/TMP_Font_AssetBundles.zip)获取，请解压后直接放置在游戏根目录  
部分游戏`OverrideFontTextMeshPro`可能无效，可以尝试使用`FallbackFontTextMeshPro`  
此外，若出现不翻译、翻译缺失格式等问题，可尝试修改`MaxCharactersPerTranslation`和`IgnoreWhitespaceInDialogue`选项

### 完整配置示例
```
[Sakura]
Endpoint=http://127.0.0.1:8080/v1/chat/completions
ModelName=Sakura
ModelVersion=1.0
MaxTokensMode=Dynamic
StaticMaxTokens=512
DynamicMaxTokensMultiplier=1.5
DictMode=MatchOriginalText
Dict={"想太":["想太","男主人公"],"ダイヤ":["戴亚","女"]}
FixDegeneration=True
MaxConcurrency=1
EnableShortDelay=True
DisableSpamChecks=False
TemperatureOverride=
TopPOverride=
Debug=False
```

### 支持的模型及对应关系
对应参数为`ModelName`和`ModelVersion`，对应关系如下表，其中`*`表示没有匹配时的默认值

| ModelName    | ModelVersion | TranslationModel        |
|--------------|--------------|-------------------------|
| Sakura       | 0.9          | Sakura v0.9             |
| Sakura       | 0.10         | Sakura v0.10            |
| Sakura       | 1.0          | Sakura v1.0             |
| Sakura       | *            | Sakura v1.0 (默认)      |
| GalTransl    | 2.6          | GalTransl v2.6          |
| GalTransl    | 3            | GalTransl v3            |
| GalTransl    | *            | GalTransl v3 (默认)     |
| *            | *            | Sakura v1.0 (默认)      |

模型默认值为Sakura 1.0  
需要将`Endpoint`设置为chat completions api（例：`http://127.0.0.1:8080/v1/chat/completions`）  

### MaxTokens模式及退化检测配置
- `MaxTokensMode`
  - `Static`（默认）：使用`StaticMaxTokens`的值作为发送给llama.cpp的`max_tokens`
  - `Dynamic`：使用原文的字符数乘以`DynamicMaxTokensMultiplier`向上取整作为发送给llama.cpp的`max_tokens`，最小`max_tokens`为`10`
  - 设置为`Dynamic`有助于在模型退化时减少检测时间
- `StaticMaxTokens`：静态MaxTokens值，默认`512`
- `DynamicMaxTokensMultiplier`：动态MaxTokens乘以原文字符数的倍数，默认`1.5`
- `FixDegeneration`
  - `False`（默认）：关闭退化检测功能
  - `True`：开启退化检测功能，当模型生成的token数等于传递给llama.cpp的`max_tokens`时，将`frequency_penalty`增加至`0.2`重新生成，若仍然失败则使用最后获取的结果

### 字典配置
#### 字典配置项
- `DictMode`
  - `None`（默认）：关闭字典功能（注：Sakura 0.9不支持字典，启用字典功能时也不会传递字典）
  - `Full`：传递整个字典
  - `MatchOriginalText`：传递当前翻译句子包含的字典部分
#### 字典配置（Dict）
- `Dict`默认为空字符串，必须为空或合法的Json格式，解析失败将会视为空
- 字典格式`{"src":["dst","info"]}`，发给sakura的字典为`src->dst #info`
- 其中`info`没有可以写成`{"src":["dst"]}`或者`{"src":"dst"}`，此时发给sakura的字典为`src->dst`
- 示例：`{"たちばな":"橘","橘":["橘"],"あやの":["绫乃","女"],"綾乃":["绫乃","女"]}`

### 并发配置
- `MaxConcurrency`：同时发送的翻译请求数，默认为`1`
- 单卡多线程总体翻译速度一般比单线程高，llama.cpp需要设置`-np`参数（可使用`llama-batched-bench`测试）

### 快速翻译配置 :warning:
- **注意：仅建议在本地部署的情况下使用**
- `EnableShortDelay`
  - `False`（默认）：AutoTranslator默认的翻译延迟
  - `True`：设置翻译延迟为0.1秒（AutoTranslator允许的最小值）
- `DisableSpamChecks`
  - `False`（默认）：开启AutoTranslator的Spam检测
  - `True`：关闭Spam检测

### Temperature和TopP覆盖设置
- `TemperatureOverride`：覆盖模型默认的temperature值（可选，留空使用模型默认值）
  - 不同模型的默认temperature值：
	- Sakura系列：0.1
	- GalTransl v2.6：0.2
	- GalTransl v3：0.3
- `TopPOverride`：覆盖模型默认的top_p值（可选，留空使用模型默认值）
  - 不同模型的默认top_p值：
	- Sakura系列：0.3
	- GalTransl系列：0.8

## 启动游戏
完成所有配置后，请删除第一次启动时产生的翻译文件
 - 如果你使用ReiPatcher，翻译文件应该在`{游戏目录}\AutoTranslator\Translation`
 - 如果你使用BepInEx，翻译文件应该在`{游戏目录}\BepInEx\Translation`

现在你可以开始游戏了

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)
