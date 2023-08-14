from fastapi import FastAPI, Request
from transformers import AutoTokenizer, AutoModel,AutoModelForCausalLM, AutoTokenizer
import uvicorn, json, datetime
import torch
from transformers.generation import GenerationConfig
import os, platform
import shutil
import copy

DEVICE = "cuda"
DEVICE_ID = "0"
CUDA_DEVICE = f"{DEVICE}:{DEVICE_ID}" if DEVICE_ID else DEVICE

print('cuda:', torch.cuda.is_available(), CUDA_DEVICE)

if os.name == 'nt':
    DEFAULT_CKPT_PATH = 'C:\\ai\\chatgpt\\q\\Qwen-7B-Chat'
else:
    DEFAULT_CKPT_PATH = '/mnt/c/ai/chatgpt/q/Qwen-7B-Chat'
    
    
def torch_gc():
    if torch.cuda.is_available():
        with torch.cuda.device(CUDA_DEVICE):
            torch.cuda.empty_cache()
            torch.cuda.ipc_collect()


app = FastAPI()
@app.get("/config")
async def get_config(request: Request):
    global model, tokenizer
    if(model.generation_config):
        return model.generation_config
    return "{}"

@app.post("/")
async def create_item(request: Request):
    global model, tokenizer
    json_post_raw = await request.json()
    # print(json_post_raw)
    
    prompt =  json_post_raw['prompt']
    
    history=[]
    if 'history' in json_post_raw:
        history = json_post_raw['history']
    
    old_config = model.generation_config
    config = copy.copy(model.generation_config)
    
    if 'top_p' in json_post_raw:
        config.top_p = json_post_raw['top_p']
    
    if 'max_length' in json_post_raw:
        config.max_length = json_post_raw['max_length']
    
    if 'temperature' in json_post_raw:
        config.temperature = json_post_raw['temperature']

    response, history = model.chat(tokenizer, prompt, history=history)
    
    model.generation_config = old_config
    now = datetime.datetime.now()
    time = now.strftime("%Y-%m-%d %H:%M:%S")
    answer = {
        "response": response,
        "history": history,
        "status": 200,
        "time": time
    }
    # log = "[" + time + "] " + '", prompt:"' + prompt + '", response:"' + repr(response) + '"'
    # print(log)
    torch_gc()
    return answer

def _load_model_tokenizer():
    tokenizer = AutoTokenizer.from_pretrained(
        DEFAULT_CKPT_PATH, trust_remote_code=True,
    )

    model = AutoModelForCausalLM.from_pretrained(
        DEFAULT_CKPT_PATH,
        device_map=CUDA_DEVICE,
        fp16=True,
        trust_remote_code=True,
    ).cuda().eval()
    
    config =  GenerationConfig.from_pretrained(
        DEFAULT_CKPT_PATH, trust_remote_code=True
        )
    config.max_new_tokens = 8192
    config.max_context_size = 8192
    config.max_generate_size = 8192

    model.generation_config = config
    print('model.config:', model.generation_config )
    return model, tokenizer


if __name__ == '__main__':

    model, tokenizer = _load_model_tokenizer()
    uvicorn.run(app, host='0.0.0.0', port=8001, workers=1)
