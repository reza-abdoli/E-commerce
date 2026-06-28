const PAGE_TITLES = {
  colors:     'مدیریت رنگ‌ها',
  brands:     'مدیریت برند‌ها',
  categories: 'مدیریت دسته‌بندی‌ها',
  products:   'مدیریت محصولات',
  users:      'مدیریت کاربران',
  home:       'داشبورد',
};

let colorCount   = 4;
let brandCount   = 2;
let productCount = 2;

/* ─── Navigation ─── */
function nav(page, el) {
  document.querySelectorAll('.sb-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  document.getElementById('page-' + page).classList.add('active');
  document.getElementById('topbar-title').textContent = PAGE_TITLES[page] || '';
}

function toggleGroup(id) {
  const el   = document.getElementById(id);
  const chev = document.getElementById('chev-' + id);
  const open = el.style.display !== 'none';
  el.style.display = open ? 'none' : '';
  chev.classList.toggle('open', !open);
}

/* ─── Generic helpers ─── */
function delRow(btn) {
  btn.closest('tr').remove();
}

function showAlert(id) {
  const el = document.getElementById(id);
  el.style.display = 'flex';
  setTimeout(() => { el.style.display = 'none'; }, 2400);
}

function toggleForm(id) {
  const f = document.getElementById(id);
  f.style.display = (f.style.display === 'none' || f.style.display === '') ? 'block' : 'none';
}

function previewImg(input, previewId) {
  const p = document.getElementById(previewId);
  if (input.files && input.files[0]) {
    const r = new FileReader();
    r.onload = e => {
      p.src = e.target.result;
      p.style.display = 'block';
    };
    r.readAsDataURL(input.files[0]);
  }
}

/* ─── Toggle component ─── */
function initToggle(toggleId, labelId) {
  const tog = document.getElementById(toggleId);
  const lbl = document.getElementById(labelId);
  if (!tog) return;
  tog.addEventListener('click', () => {
    tog.classList.toggle('on');
    if (lbl) lbl.textContent = tog.classList.contains('on') ? 'بله' : 'خیر';
  });
}

/* ─── Color picker sync ─── */
function initColorPicker(textId, pickerId) {
  const text   = document.getElementById(textId);
  const picker = document.getElementById(pickerId);
  if (!text || !picker) return;

  picker.addEventListener('input', () => { text.value = picker.value; });
  text.addEventListener('input', () => {
    if (/^#[0-9a-f]{6}$/i.test(text.value)) picker.value = text.value;
  });
}

/* ─── Save: Color ─── */
function saveColor() {
  const title = document.getElementById('c-title').value.trim();
  const code  = document.getElementById('c-code').value.trim()
              || document.getElementById('c-picker').value;
  if (!title) { document.getElementById('c-title').focus(); return; }

  colorCount++;
  const dot = `<span class="color-dot" style="background:${code}"></span>`;
  appendRow('color-tbody', [
    colorCount,
    title,
    dot,
    `<span class="mono">${code}</span>`,
    colorCount + 1,
    actionBtns(),
  ]);

  document.getElementById('c-title').value = '';
  document.getElementById('c-code').value  = '';
  showAlert('alert-color');
}

/* ─── Save: Brand ─── */
function saveBrand() {
  const title = document.getElementById('b-title').value.trim();
  const url   = document.getElementById('b-url').value.trim();
  const order = document.getElementById('b-order').value || '—';
  if (!title) { document.getElementById('b-title').focus(); return; }

  brandCount++;
  const thumb = `<div class="img-thumb" style="background:#334155;color:#c9d1d9;font-size:9px;font-weight:600">${title.substring(0, 4)}</div>`;
  appendRow('brand-tbody', [
    brandCount,
    thumb,
    title,
    `<span class="txt-link mono">${url}</span>`,
    `<span style="text-align:center;display:block">${order}</span>`,
    brandCount,
    actionBtns(),
  ]);

  ['b-title','b-url','b-order'].forEach(id => {
    document.getElementById(id).value = '';
  });
  showAlert('alert-brand');
}

/* ─── Save: Category ─── */
function saveCat() {
  const title = document.getElementById('cat-title').value.trim();
  const url   = document.getElementById('cat-url').value.trim();
  if (!title) { document.getElementById('cat-title').focus(); return; }

  const isHome = document.getElementById('tog-home').classList.contains('on');
  const homeIcon = isHome
    ? `<i class="ti ti-circle-check icon-ok" aria-label="بله"></i>`
    : `<i class="ti ti-x icon-no" aria-label="خیر"></i>`;
  const parentSel = document.getElementById('cat-parent');
  const parentText = parentSel.options[parentSel.selectedIndex]?.text || '—';
  const parentVal  = parentSel.value ? parentText : '<span style="color:var(--text-muted)">—</span>';

  appendRow('cat-tbody', [
    '', title,
    `<span class="txt-link mono">${url}</span>`,
    parentVal,
    document.getElementById('cat-order').value || '—',
    `<span style="text-align:center;display:block">${homeIcon}</span>`,
    actionBtns(),
  ]);

  document.getElementById('cat-title').value = '';
  document.getElementById('cat-url').value   = '';
  showAlert('alert-cat');
}

/* ─── Save: Product ─── */
function saveProduct() {
  const title = document.getElementById('p-title').value.trim();
  const price = document.getElementById('p-price').value;
  if (!title) { document.getElementById('p-title').focus(); return; }

  productCount++;
  const priceStr = price
    ? Number(price).toLocaleString('fa') + ' تومان'
    : '—';

  appendRow('product-tbody', [
    productCount,
    `<div class="img-thumb"><i class="ti ti-shirt" style="color:var(--text-muted);font-size:18px" aria-hidden="true"></i></div>`,
    `<span class="txt-link">${title}</span>`,
    '—',
    `<span style="font-size:12px">${priceStr}</span>`,
    '<span style="text-align:center;display:block">۰</span>',
    productCount,
    actionBtns(),
  ]);

  document.getElementById('p-title').value = '';
  document.getElementById('p-price').value = '';
  showAlert('alert-product');
}

/* ─── Helpers ─── */
function actionBtns() {
  return `<button class="badge-edit">ویرایش</button> <button class="badge-del" onclick="delRow(this)">حذف</button>`;
}

function appendRow(tbodyId, cells) {
  const tbody = document.getElementById(tbodyId);
  const tr    = document.createElement('tr');
  tr.innerHTML = cells.map(c => `<td>${c}</td>`).join('');
  tbody.appendChild(tr);
}

/* ─── Init ─── */
document.addEventListener('DOMContentLoaded', () => {
  initColorPicker('c-code', 'c-picker');
  initToggle('tog-home',  'tog-home-label');
  // initToggle('tog-avail', 'tog-avail-label');
});
