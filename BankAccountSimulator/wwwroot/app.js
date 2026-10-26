const $ = id => document.getElementById(id);
const make = (tag, text='', className='') => {const node=document.createElement(tag);node.textContent=text;node.className=className;return node;};
const date = value => new Date(value).toLocaleDateString(undefined,{month:'short',day:'numeric',year:'numeric'});
let busy=false;
function message(text,error=false){$('status').textContent=text;$('status').classList.toggle('error',error);}
async function api(path,method='GET',body){
 let response;
 try {response=await fetch(path,{method,headers:{'Content-Type':'application/json'},...(body===undefined?{}:{body:JSON.stringify(body)})});}
 catch {throw new Error('Cannot reach the app. Check that its server is running, then try again.');}
 if(response.status===204)return null;
 let data;try{data=await response.json();}catch{throw new Error('The request could not be processed. Check your values and try again.');}
 if(!response.ok)throw new Error(data.error||'The request could not be completed. Please try again.');
 return data;
}
async function run(action){
 if(busy)return;busy=true;
 const controls=[...document.querySelectorAll('button,input,select')].map(node=>[node,node.disabled]);
 controls.forEach(([node])=>node.disabled=true);
 document.querySelector('main').setAttribute('aria-busy','true');
 try{await action();}catch(error){message(error.message,true);}
 finally{busy=false;controls.forEach(([node,disabled])=>{if(node.isConnected)node.disabled=disabled;});document.querySelector('main').removeAttribute('aria-busy');}
}
function button(label,handler,className='icon-button'){
 const node=make('button',label,className);node.type='button';node.addEventListener('click',()=>{if(!busy)handler();});return node;
}
function selectButtons(selector,selected){document.querySelectorAll(selector).forEach(b=>{const active=b===selected;b.classList.toggle('selected',active);b.setAttribute('aria-pressed',String(active));});}
let type='deposit';const money=value=>new Intl.NumberFormat(undefined,{style:'currency',currency:'USD'}).format(value);
async function load(){const data=await api('/api/account');$('balance').textContent=money(data.balance);$('deposits').textContent=money(data.deposits);$('withdrawals').textContent=money(data.withdrawals);$('empty').hidden=data.transactions.length>0;$('transactions').replaceChildren(...data.transactions.map(t=>{const out=t.type==='withdraw';const row=make('div','','transaction'),details=make('div','','transaction-details');details.append(make('strong',out?'Withdrawal':'Deposit'),make('p',`${date(t.date)}${t.note?' · '+t.note:''}`),make('p',`Balance after: ${money(t.balanceAfter)}`));row.append(make('span',out?'↗':'↙',`transaction-icon ${out?'out':''}`),details,make('span',`${out?'−':'+'}${money(t.amount)}`,`amount ${out?'out':''}`));return row;}));}
document.querySelectorAll('[data-type]').forEach(b=>b.addEventListener('click',()=>{if(busy)return;type=b.dataset.type;selectButtons('[data-type]',b);$('transact').textContent=type==='deposit'?'Make deposit →':'Make withdrawal →';}));
$('transaction-form').addEventListener('submit',event=>{event.preventDefault();run(async()=>{const transaction=await api('/api/transactions','POST',{type,amount:Number($('amount').value),note:$('note').value.trim()});$('transaction-form').reset();await load();message(`${type==='deposit'?'Deposit':'Withdrawal'} of ${money(transaction.amount)} recorded.`);});});
run(async()=>{await load();message('Your simulated account is ready.');});
