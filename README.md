<div align="center">
<h1>
  SakuraTranslator
</h1>
</div>

# 介绍
这是一个基于XUnity.AutoTranslator和Sakura模型的Unity游戏本地翻译器,能够提供高质量离线日文翻译  
建议使用[Galtransl-v2.6翻译模型](https://github.com/SakuraLLM/Sakura-13B-Galgame)，当前支持版本为Sakura v0.8/v0.9/v0.10/v1.0,GalTrans2.6

## TODO
- [ ] 添加退化检测（搁置，较新的模型基本不需要）
- [ ] 添加历史上文（搁置，难以将对话文本与ui文本区分，需要更好的规则）
- [x] 重新整理对不同模型的支持


## 详细部署流程
如果你不了解autotraslator，请从头阅读部署教程：详见[本仓库wiki](https://github.com/fkiliver/SakuraTranslator/wiki)  


# 快速开始
## 启动Sakura服务
此处以本地部署为例  
确保Sakura服务器成功启动并监听http://127.0.0.1:8080 (端口根据你的启动方式而定，但要配置文件与之保持一致)

## 修改配置文件
### 安装SakuraTranslator
从[Releases](https://github.com/fkiliver/SakuraTranslator/releases) 下载SakuraTranslate.dll放置在Translators文件夹内
 - 如果你使用ReiPatcher，你应该放在`{游戏目录}\{游戏名}_Data\Managed\Translators`
 - 如果你使用BepInEx,你应该放在`{游戏目录}\BepInEx\plugins\XUnity.AutoTranslator\Translators`
### 配置SakuraTranslator
启动一次游戏，这时应当会自动生成配置文件  
修改AutoTranslator的配置文件  
 - 如果你使用ReiPatcher，配置文件应该在`{游戏目录}\AutoTranslator\Config.ini`
 - 如果你使用BepInEx,配置文件应该在`{游戏目录}\BepInEx\config\AutoTranslatorConfig.ini`

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
OverrideFontTextMeshPro= arialuni_sdf_u2018 ##或arialuni_sdf_u2019
```
其中arialuni_sdf_u201x可以从[字体文件](https://github.com/bbepis/XUnity.AutoTranslator/releases/download/v5.3.0/TMP_Font_AssetBundles.zip)获取，请解压后直接放置在游戏根目录

### 完整配置示例
```
[Sakura]
Endpoint=http://127.0.0.1:8080/v1/chat/completions
ModelName=Sakura
ModelVersion=1.0
MaxConcurrency=2
UseDict=True
DictMode=Full
Dict={"アイリス":["艾莉斯","女"]}
```

### 支持的模型及对应关系
将原`ApiType`拆成`ModelName`和`ModelVersion`，对应关系如下表，其中`*`表示没有匹配时的默认值
| ModelName  | ModelVersion | TranslationModel       |
|------------|--------------|------------------------|
| Sakura     | 0.8          | Sakura 0.8             |
| Sakura     | 0.9          | Sakura/Sakura32B 0.9*             |
| Sakura     | 0.10         | Sakura 0.10pre1            |
| Sakura     | 1.0          | Sakura 1.0             |
| Sakura     | *            | Sakura 1.0 (默认)      |
| Sakura32B  | 0.10         | Sakura32B 0.10         |
| Sakura32B  | *            | Sakura32B 0.10 (默认)  |
| GalTransl  | 2.6          | GalTransl 2.6          |
| GalTransl  | *            | GalTransl 2.6 (默认)   |
| *          | *            | Sakura 1.0 (默认)      |

模型相关默认值设置为Sakura 1.0，注意Sakura32B 0.9*需将`ModelName`设置为`Sakura`

### 字典
#### 字典配置项
- `UseDict`默认为`False`，设置为`True`才会启用字典功能
- `DictMode`默认为`Full`，为`Full`时传递整个字典，为`Partial`或其他时，传递当前翻译句子包含的字典部分
- `Dict`默认为空字符串
#### 字典配置（Dict）
- 必须为空或合法的Json格式，解析失败将会视为空
- 字典格式`{"src":["dst","info"]}`，发给sakura的字典为`src->dst #info`
- 其中`info`没有可以写成`{"src":["dst"]}`或者`{"src":"dst"}`，此时发给sakura的字典为`src->dst`
- 示例：`{"たちばな":"橘","橘":["橘"],"あやの":["绫乃","女"],"綾乃":["绫乃","女"]}`

### 其他问题
- `frequency_penalty`目前设置为`0.2`，暂时没有加入退化检测
- 可以使用并发参数MaxConcurrency，单卡多线程总体翻译速度比单线程高（3090，1线程约50t/s，5线程约5x35t/s）    


## 启动游戏
完成所有配置后，请删除第一次启动时产生的翻译文件
 - 如果你使用ReiPatcher，翻译文件应该在`{游戏目录}\AutoTranslator\Translation
 - 如果你使用BepInEx,翻译文件应该在`{游戏目录}\BepInEx\Translation

现在你可以开始游戏了

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)



![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)
