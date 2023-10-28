# llms_test

项目主要是对一些llms开源模型，做翻译和总结的测试

目前已测试的模型有：

```
ChatGLM2
Qwen-7B
YuLan-13B-Chat(int8)
baichuan-13B-Chat(int8)
```

测试原文:   ./txt/chinadaily
测试结果：  ./txt/对应模型名称

[2023/10/28]
1. 增加 chatglm3-6b 的翻译测试，这次测试下来，新闻类的翻译有退化了。看介绍，新版本增强了指令方面能力，适合做 Agent 任务。

[2023/10/19]
1. 增加了提示词测试翻译的代码，
2. 增加 chatglm2 baichuan2 qwen 对于带有代码段落文章的测试用例。
    目前测试下来
    baichuan2 对于给出的翻译提示词无任何效果。
    chatglm2 能够部分实现不翻译代码的效果，但还不够好
    qwen 的效果是目前看到的最好表现

[2023/9/28]
1. 修改了一下测试用例的方法，针对同一篇文章，在翻译时，可以输入不同的top_p和temperature，已对比不同的结果。
2. 增加了baichuan2-13B-Chat 的测试用例。翻译的质量也不错，处理的耗时比baichuan-13B-Chat要快一些。
3. 补测了 ChatGLM 模型在不同的top_p值下的结果。发现翻译内容比之前差很多，不清楚上一次是用ChatGLM2测试的，还是用ChatGLM1测试的。相同文章，翻译时间只有baichuan2-13B的1/4. 增加qwen-14b-int4 的测试用例，翻译质量看原文。翻译速度上，比chatglm慢，但比baicuan2快。


测试环境：

```

主机：x99 2680v4 + 64G ddr4
显卡：2080ti 22G
操作系统：
win10 22H2 19045.2846
wsl下 linux 22.04

```

测试结果里会当前模型的执行性能，仅供参考。

目前 baichuan 和 yulan qwen 仅能在 linux下运行，因此性能部分仅限 wsl 的性能，实际性能可能会更高。

