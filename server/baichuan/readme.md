
## 安装时用到的脚本

```

conda create -n baichuan python=3.10 -y

conda activate baichuan

pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

pip install uvicorn
pip install fastapi
pip install -r requirements.txt

pip install bitsandbytes
pip install scipy

```

## 模型文件

```

# 进入要下载模型的目录，从 hf 上下模型

https://huggingface.co/baichuan-inc/Baichuan-13B-Chat


```


## 运行时用到的脚本


```

# 模型只能在linux下运行，目前测试环境是 wsl
# 需要修改 api.py 你模型文件位置

python api.py


```