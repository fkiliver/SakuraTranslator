# 介绍
这是一个基于XUnity.AutoTranslator和Sakura模型的Unity游戏本地翻译器  
# 准备
首先参考XUnity.AutoTranslator文档部署XUnity.AutoTranslator：[XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator)  
然后参考Sakura模型文档完成本地部署：[Sakura模型本地部署教程](https://books.fishhawk.top/forum/656d60530286f15e3384fcf8)  
# 流程
确保Sakura服务器成功启动并监听http://127.0.0.1:8080  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/a69e74a6-f789-4de2-9ce5-d73209f2843c)

从[Releases](https://github.com/fkiliver/SakuraTranslator/releases) 下载SakuraTranslator.dll放置在Translators文件夹内

修改AutoTranslatorConfig.ini中
[General]
Language=zh
FromLanguage=ja

启动游戏后，使用快捷键alt+0打开翻译面板，选择SakuraTranslator  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/17c2c144-dab7-4b23-958f-a0dd8ddd11d4)

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/ffba161d-8d0c-4a0e-bd15-71ab95db30ef)
