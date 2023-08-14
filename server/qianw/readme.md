
## 安装时用到的脚本

```

conda create -n qwen python=3.10 -y

conda activate qwen

pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

pip install uvicorn
pip install fastapi
pip install -r requirements.txt


```
## 模型文件

```

# 进入要下载模型的目录，从 hf 上下模型

git clone https://huggingface.co/Qwen/Qwen-7B-Chat


```


## 运行时用到的脚本

```

# 需要修改代码里模型的位置

python api.py


```