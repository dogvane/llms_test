import os, platform, copy
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer
from transformers.generation.utils import GenerationConfig
import uvicorn, json
from fastapi import FastAPI, Request
from datetime import datetime

DEVICE = "cuda"
DEVICE_ID = "0"
CUDA_DEVICE = f"{DEVICE}:{DEVICE_ID}" if DEVICE_ID else DEVICE

print('cuda:', torch.cuda.is_available(), CUDA_DEVICE)

if os.name == 'nt':
    DEFAULT_CKPT_PATH = 'C:\\ai\\chatgpt\\Baichuan\\models\\Baichuan-13B-Chat'
else:
    DEFAULT_CKPT_PATH = '/mnt/c/ai/chatgpt/Baichuan/models/Baichuan-13B-Chat'
    
def torch_gc():
    if torch.cuda.is_available():
        with torch.cuda.device(CUDA_DEVICE):
            torch.cuda.empty_cache()
            torch.cuda.ipc_collect()

app = FastAPI()

def init_model():
    print("init model ...")

    model = AutoModelForCausalLM.from_pretrained(
        DEFAULT_CKPT_PATH,        
        trust_remote_code=True,
        device_map="cuda:0", 
        load_in_8bit=True
    )

    # model = AutoModelForCausalLM.from_pretrained(
    #     DEFAULT_CKPT_PATH,        
    #     trust_remote_code=True,
    #     device_map="cuda:0", 
    #     torch_dtype=torch.float16
    # ).quantize(8).cuda().eval()
    
    print("init tokenizer ...")
    tokenizer = AutoTokenizer.from_pretrained(
        DEFAULT_CKPT_PATH,
        device_map="cuda:0", 
        trust_remote_code=True,
        load_in_8bit=True
    )
    
    model.generation_config = GenerationConfig.from_pretrained(
        DEFAULT_CKPT_PATH
    )
    
    print(model.generation_config)
    model.generation_config.max_new_tokens = 8192
    model.generation_config.max_context_size = 8192
    model.generation_config.max_generate_size = 8192

    return model, tokenizer

@app.post("/")
async def run_chat(request: Request):
    global model, tokenizer
    json_post_raw = await request.json()
    # print(json_post_raw)
    json_post = json.dumps(json_post_raw)
    json_post_list = json.loads(json_post)
    prompt = json_post_list.get('prompt')
    history = json_post_list.get('history')
    if (history == None):
        history=[]
    
    config = copy.copy(model.generation_config)
    
    if 'top_p' in json_post_raw:
        config.top_p = json_post_raw['top_p']
    
    if 'max_length' in json_post_raw:
        config.max_length = json_post_raw['max_length']
    
    if 'temperature' in json_post_raw:
        config.temperature = json_post_raw['temperature']
    
    messages = []
    messages.append({"role": "user", "content": prompt})
    
    start_time = datetime.now()  # 记录启动时间
    
    response = model.chat(tokenizer, messages, generation_config = config)
    print(response)
    if torch.backends.mps.is_available():
        torch.mps.empty_cache()
    
    now = datetime.now()
    time = now.strftime("%Y-%m-%d %H:%M:%S")
    answer = {
        "response": response,
        "history": history,
        "status": 200,
        "time": time
    }
    
    print(f"run request.len{len(prompt)} time:{(datetime.now() - start_time).total_seconds()} seconds. max_length:{config.max_length} top:{config.top_p} temperature:{config.temperature}")
    
    torch_gc()
    return answer


if __name__ == "__main__":
    start_time = datetime.now()  # 记录启动时间

    model, tokenizer = init_model()

    print(f"Model initialized in {(datetime.now() - start_time).total_seconds()} seconds.")
    
    uvicorn.run(app, host='0.0.0.0', port=8001, workers=1)
