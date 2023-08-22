from fastapi import FastAPI, Request
from transformers import AutoModelForCausalLM, AutoTokenizer
from transformers.utils import logging
from transformers.generation.utils import LogitsProcessorList, StoppingCriteriaList
from transformers.generation import GenerationConfig
from dataclasses import dataclass, asdict
from tools.transformers.interface import generate_interactive, GenerationConfig

import uvicorn, json, datetime
import torch
import os, platform
import shutil
import copy

DEVICE = "cuda"
DEVICE_ID = "0"
CUDA_DEVICE = f"{DEVICE}:{DEVICE_ID}" if DEVICE_ID else DEVICE

print('cuda:', torch.cuda.is_available(), CUDA_DEVICE)

if os.name == 'nt':
    DEFAULT_CKPT_PATH = 'C:\\ai\\chatgpt\\InternLM\\models\\internlm-chat-7b'
else:
    DEFAULT_CKPT_PATH = '/mnt/c/ai/chatgpt/InternLM/models/internlm-chat-7b'
    
    
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

user_prompt = "<|User|>:{user}<eoh>\n"
robot_prompt = "<|Bot|>:{robot}<eoa>\n"
cur_query_prompt = "<|User|>:{user}<eoh>\n<|Bot|>:"

def buildPrompt(prompt):
    total_prompt = ""
    total_prompt = total_prompt + cur_query_prompt.replace("{user}", prompt)
    return total_prompt


@app.post("/")
async def create_item(request: Request):
    global model, tokenizer
    json_post_raw = await request.json()
    # print(json_post_raw)
    
    inputText =  json_post_raw['prompt']
    
    history=[]
    if 'history' in json_post_raw:
        history = json_post_raw['history']
    
    max_length = 2048
    top_p = 0.9
    temperature = 0.9
    
    if 'top_p' in json_post_raw:
        top_p = json_post_raw['top_p']
    
    if 'max_length' in json_post_raw:
        max_length = json_post_raw['max_length']
    
    if 'temperature' in json_post_raw:
        temperature = json_post_raw['temperature']

    print(f'prompt.len:{len(inputText)}    top_p:{top_p}    max_length:{max_length}    temperature:{temperature}')
    
    generation_config = GenerationConfig(
        max_length=max_length,
        top_p=top_p,
        temperature=temperature
    )
    inputText = buildPrompt(inputText)
    response = generate_interactive(model=model, tokenizer=tokenizer, 
                                    prompt=inputText, 
                                    additional_eos_token_id=103028, **asdict(generation_config))
    
    last_response = None
    for response in response:
        last_response = response

    print(last_response)    
    
    now = datetime.datetime.now()
    time = now.strftime("%Y-%m-%d %H:%M:%S")
    answer = {
        "response": last_response,
        "history": history,
        "status": 200,
        "time": time
    }
    # log = "[" + time + "] " + '", prompt:"' + prompt + '", response:"' + repr(response) + '"'
    # print(log)
    torch_gc()
    return answer

def _load_model_tokenizer():
    model = AutoModelForCausalLM.from_pretrained(DEFAULT_CKPT_PATH, trust_remote_code=True).to(torch.bfloat16).cuda()
    tokenizer = AutoTokenizer.from_pretrained(DEFAULT_CKPT_PATH, trust_remote_code=True)
    
    # config =  GenerationConfig.from_pretrained(
    #     DEFAULT_CKPT_PATH, trust_remote_code=True
    #     )
    # config.max_new_tokens = 8192
    # config.max_context_size = 8192
    # config.max_generate_size = 8192

    # model.generation_config = config
    print('model.config:', model.generation_config )
    
    return model, tokenizer


if __name__ == '__main__':

    model, tokenizer = _load_model_tokenizer()
    uvicorn.run(app, host='0.0.0.0', port=8001, workers=1)
