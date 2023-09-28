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

测试结果好坏不做评价，可进入自行判断。每个模型下不同的top_p和temperature结果会不太一样，测试文本里，尽量将top_p和temperature设置为0.9，方便对比。

实际使用中，需要根据实际模型用途进行修改。

测试环境：

```

主机：x99 2680v4 + 64G ddr4
显卡：2080ti 22G
操作系统：
win10 22H2 19045.2846
wsl下 linux 22.04

```

测试结果里会当前模型的执行性能，仅供参考。

目前 baichuan 和 yulan 仅能在 linux下运行，因此性能部分仅限 wsl 的性能，实际性能会更高。

[2023/9/28]
1. 修改了一下测试用例的方法，针对同一篇文章，在翻译时，可以输入不同的top_p和temperature，已对比不同的结果。
2. 增加了baichuan2-13B-Chat 的测试用例。翻译的质量也不错，处理的耗时比baichuan-13B-Chat要快一些。
3. 补测了 ChatGLM 模型在不同的top_p值下的结果。发现翻译内容比之前差很多，不清楚上一次是用ChatGLM2测试的，还是用ChatGLM1测试的。相同文章，翻译时间只有baichuan2-13B的1/4. 增加qwen-14b-int4 的测试用例，翻译质量看原文。翻译速度上，比chatglm慢，但比baicuan2快。

