# 使用CustomEndpoint调用SakuraLLM

**注意，此方法为进阶方法！建议对大语言模型比较了解、有一定Python基础的人使用**

AutoTranslator为了防止发出过多请求，会在游戏内文本稳定1秒不变后再发送。不过AutoTranslator自带一个CustomEndpoint，本意是用来调试，但如果可以自己编写一个简单的API，便可以大幅提高翻译效率。

由于是在服务端而非客户端进行额外处理，所以不需要重新编译DLL，优点就是可以比较自由的增加一些功能，比如翻译文本缓存、上下文、字典等，缺点就是需要你懂不少前置知识……

## AutoTranslator设置

CustomEndpoint为插件自带，并不需要下载任何额外的dll即可使用。

```ini
[Service]
Endpoint=CustomTranslate

[Custom]
Url=http://127.0.0.1:8000
EnableShortDelay=True
DisableSpamChecks=False
```

只需要这样简单设置，即可以将稳定延迟设置为0.1秒，基本上可以做到立即翻译。

## API编写

这种方式的HTTP请求是非常粗放的GET请求，这里使用一个非常简单的FastAPI示例来处理请求：

```python
from llm import translate
from fastapi import FastAPI
import uvicorn

app = FastAPI()
# 用于设定专有名词的字典表
dicts = [
    {"src": "プリシア", "dst": "普莉西亚"}
]

@app.get("/")
def read_item(text: str):
    print(text)
    result = translate(text)
    print(result)
    return result

if __name__ == '__main__':
    uvicorn.run(app)
```

这里的llm是我自己写的一个简单的SakuraLLM调用脚本：

```python
from llama_cpp import Llama
llm = Llama("sakura-7b-qwen2.5-v1.0-q6k.gguf", n_gpu_layers=-1, verbose=False)
system_prompt = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。"

def translate(text: str, gpt_dicts: list[dict] = []) -> str:
    """
    ```json
    gpt_dicts = [{
        "src": "原文",
        "dst": "译文",
        "info": "注释信息(可选)"
    }, ...]
    ```
    """
    if len(gpt_dicts) == 0:
        user_prompt = "将下面的日文文本翻译成中文：" + text
    else:
        user_prompt = "根据以下术语表（可以为空）：\n"
        for gpt in gpt_dicts:
            src = gpt['src']
            dst = gpt['dst']
            info = gpt['info'] if "info" in gpt.keys() else None
            if info:
                single = f"{src}->{dst} #{info}\n"
            else:
                single = f"{src}->{dst}\n"
            user_prompt += single
        user_prompt += "将下面的日文文本根据对应关系和备注翻译成中文：" + text
    res = llm.create_chat_completion(messages=[
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": user_prompt}
    ], temperature=0.1, top_p=0.3, repeat_penalty=1, max_tokens=512, frequency_penalty=0.2)
    return res["choices"][0]["message"]["content"]

if __name__ == "__main__":
    while True:
        raw = input("待翻译语句: ")
        dicts = [{
            "src": "一ノ瀬　雫",
            "dst": "一之濑-雫",
        }]
        print("翻译结果:", translate(raw, dicts))
```
