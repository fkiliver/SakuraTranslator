# 介绍
这是一个基于Mtool和Sakura模型的RPGMaker游戏本地翻译器  
通过Mtool导入导出翻译文本，通过Sakura模型翻译日语文本  
# 准备
首先参考Mtool文档完成对翻译文本的导出：[Mtool](https://afdian.net/a/AdventCirno)  
然后参考Sakura模型文档完成本地部署：[Sakura模型本地部署教程](https://books.fishhawk.top/forum/656d60530286f15e3384fcf8)  
# 流程
确保Sakura服务器成功启动并监听http://127.0.0.1:8080  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/a69e74a6-f789-4de2-9ce5-d73209f2843c)

使用MTool启动游戏并导出待翻译的原文  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/bc00335f-751e-4252-98bc-8b807640c400)

复制项目内的main.py文件放置在Mtool导出的ManualTransFile.json同级目录下  

运行main.py
```
python main.py
```
程序会自动开始翻译同级目录下的ManualTransFile.json  

![image](https://github.com/fkiliver/SakuraTranslator/assets/48873439/8699c9c8-ba52-43af-8a42-c86686340ff1)

每翻译100行会保存当前翻译进度，下次启动翻译器时会从中断位置继续翻译

翻译完成后通过Mtool加载翻译文件ManualTransFile.json即可完成翻译  
