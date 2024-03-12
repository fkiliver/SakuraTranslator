# 介绍
这是一个基于XUnity.AutoTranslator和Sakura模型的Unity游戏本地翻译器  
建议使用Sakura v0.9b https://huggingface.co/SakuraLLM/Sakura-13B-LNovel-v0.9b-GGUF/tree/main  
# 准备
首先参考XUnity.AutoTranslator文档部署XUnity.AutoTranslator：[XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator)  
然后参考Sakura模型文档完成本地部署：[Sakura模型本地部署教程](https://github.com/SakuraLLM/Sakura-13B-Galgame/wiki)  
# 流程
确保Sakura服务器成功启动并监听http://127.0.0.1:8080  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/a69e74a6-f789-4de2-9ce5-d73209f2843c)

从[Releases](https://github.com/fkiliver/SakuraTranslator/releases) 下载SakuraTranslator.dll放置在Translators文件夹内

修改AutoTranslatorConfig.ini

添加配置（仅在`OpenAI`模式下生效）

- `UseDict`默认为空字符串
- `DictMode`默认为`Full`
- `Dict`默认为空字符串

其中

 - `UseDict`为false时是老的OpenAI Prompt
 - `DictMode`为`Full`时传递整个字典，为`Partial`或其他时，传递当前翻译句子包含的字典部分
 - `Dict`为json编码的字符串，格式同MTool，为`{"k1":"v1","k2":"v2"}`，暂未发现SakuraLLM官方示例中给的字典注释有什么作用

llama.cpp-b2355，sakura-13b-qwen2beta-v0.10pre0-Q6_K.gguf，Windows和Linux下测试
理论上高版本llama.cpp和Kaggle都能用，不过我未测试Kaggle
配置：
```
[Sakura]
Endpoint=http://127.0.0.1:5000/v1/chat/completions
ApiType=OpenAI
UseDict=True
DictMode=Full
Dict={"たちばな":"橘","橘":"橘","あやの":"绫乃","綾乃":"绫乃"}
```

启动游戏后，使用快捷键alt+0打开翻译面板，选择SakuraTranslator  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/17c2c144-dab7-4b23-958f-a0dd8ddd11d4)

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)
