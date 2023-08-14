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
