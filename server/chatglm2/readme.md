
## 安装时用到的脚本

```

conda create -n chatglm python=3.10 -y

conda activate chatglm

pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

pip install uvicorn
pip install fastapi
pip install -r requirements.txt


```
## 模型文件

```

# 模型文件，使用的是 32k 窗口的长文本模型

git clone https://github.com/THUDM/ChatGLM2-6B

copy openapi_2.py ChatGLM2-6B

# 执行后，模型文件会自动更新到 ./cache目录下

```


## 运行时用到的脚本

```

# windows 下可执行

python openai_api2.py


```