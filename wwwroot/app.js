// ============================================================
//  Garage POS – app.js  (live DB version)
// ============================================================

let allProducts = [];
let cart = [];

// ── Boot ────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
    await loadProducts();
    updateCartUI();

    document.getElementById('btnClearCart').addEventListener('click', clearCart);
    document.getElementById('btnCheckout').addEventListener('click', processCheckout);

    // Search
    document.getElementById('searchInput').addEventListener('input', (e) => {
        const q = e.target.value.toLowerCase().trim();
        renderProducts(q ? allProducts.filter(p =>
            p.name.toLowerCase().includes(q) ||
            (p.barcode && p.barcode.includes(q)) ||
            (p.sku && p.sku.toLowerCase().includes(q))
        ) : allProducts);
    });

    // Category filter
    document.querySelectorAll('.cat-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.cat-btn').forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');
            const cat = e.target.innerText.trim();
            renderProducts(cat === 'All Parts' ? allProducts : allProducts.filter(p => p.category === cat));
        });
    });

    setupBarcodeScanner();
});

// ── Fetch products from real API ────────────────────────────
async function loadProducts() {
    try {
        const res = await fetch('/api/products');
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        allProducts = await res.json();
        renderProducts(allProducts);
        checkLowStockAlerts();
        updateCategoryButtons();
    } catch (err) {
        console.error('Failed to load products:', err);
        document.getElementById('productGrid').innerHTML =
            `<p style="color:#ef4444;padding:20px">⚠️ Could not connect to server.<br>${err.message}</p>`;
    }
}

// ── Dynamically populate category buttons from real data ───
function updateCategoryButtons() {
    const cats = ['All Parts', ...new Set(allProducts.map(p => p.category))];
    const container = document.querySelector('.categories');
    container.innerHTML = '';
    cats.forEach((cat, i) => {
        const btn = document.createElement('button');
        btn.className = 'cat-btn' + (i === 0 ? ' active' : '');
        btn.textContent = cat;
        btn.addEventListener('click', () => {
            document.querySelectorAll('.cat-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            renderProducts(cat === 'All Parts' ? allProducts : allProducts.filter(p => p.category === cat));
        });
        container.appendChild(btn);
    });
}

// ── Render product grid ─────────────────────────────────────
function renderProducts(products) {
    const grid = document.getElementById('productGrid');
    grid.innerHTML = '';

    if (products.length === 0) {
        grid.innerHTML = '<p style="color:#6b7280;padding:20px">No items found.</p>';
        return;
    }

    products.forEach(p => {
        const isLow  = !p.isService && p.stock > 0 && p.stock <= (p.minStock || 5);
        const isOut  = !p.isService && p.stock === 0;

        const card = document.createElement('div');
        card.className = 'product-card';
        if (isOut) card.style.opacity = '0.5';

        const icon = p.isService ? '🔧' : getCategoryIcon(p.category);

        card.innerHTML = `
            <div class="product-img">${icon}</div>
            <div class="product-info">
                <h3>${p.name}</h3>
                <p>${p.category}</p>
            </div>
            <div class="product-price">
                <span class="price">$${Number(p.price).toFixed(2)}</span>
                <span class="stock-badge ${isLow || isOut ? 'low' : ''}">
                    ${p.isService ? 'Service' : (isOut ? 'Out of Stock' : (isLow ? `Low: ${p.stock}` : `Stock: ${p.stock}`))}
                </span>
            </div>
        `;

        if (!isOut) {
            card.addEventListener('click', () => addToCart(p));
        }
        grid.appendChild(card);
    });
}

function getCategoryIcon(cat) {
    const map = { Engine: '⚙️', Brakes: '🛑', Suspension: '🔩', Electrical: '⚡',
                  Body: '🚗', Interior: '💺', Accessories: '🧰', Services: '🔧', General: '📦' };
    return map[cat] || '📦';
}

// ── Barcode scanner ─────────────────────────────────────────
let barcodeBuffer = '';
let barcodeTimer  = null;

function setupBarcodeScanner() {
    document.addEventListener('keydown', (e) => {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;
        if (barcodeTimer) clearTimeout(barcodeTimer);

        if (e.key === 'Enter') {
            if (barcodeBuffer.length > 3) handleBarcodeScan(barcodeBuffer);
            barcodeBuffer = '';
            e.preventDefault();
            return;
        }
        if (e.key.length === 1) barcodeBuffer += e.key;
        barcodeTimer = setTimeout(() => { barcodeBuffer = ''; }, 50);
    });
}

function handleBarcodeScan(barcode) {
    const product = allProducts.find(p => p.barcode === barcode);
    if (product) {
        if (product.stock > 0 || product.isService) addToCart(product);
        else showToast(`⛔ Out of Stock: ${product.name}`, 'error');
    } else {
        showToast(`❓ Barcode not found: ${barcode}`, 'warn');
    }
}

// ── Cart ────────────────────────────────────────────────────
function addToCart(product) {
    const existing = cart.find(i => i.id === product.id);
    if (existing) {
        if (product.isService || existing.qty < product.stock) existing.qty++;
    } else {
        cart.push({ ...product, qty: 1 });
    }
    updateCartUI();
}

function updateCartQty(id, delta) {
    const item = cart.find(i => i.id === id);
    if (!item) return;
    item.qty += delta;
    if (item.qty <= 0) cart = cart.filter(i => i.id !== id);
    updateCartUI();
}

function clearCart() { cart = []; updateCartUI(); }

// ── Checkout (real API call) ────────────────────────────────
async function processCheckout() {
    if (cart.length === 0) return;

    const btn = document.getElementById('btnCheckout');
    btn.disabled = true;
    btn.textContent = 'PROCESSING…';

    try {
        const payload = { items: cart.map(i => ({ id: i.id, name: i.name, price: i.price, qty: i.qty })) };

        const res = await fetch('/api/checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err);
        }

        const result = await res.json();

        showToast(`✅ Sale Complete! Order #${result.orderId} — $${Number(result.total).toFixed(2)}`, 'success');

        clearCart();

        // Refresh product list so stock counts update live
        await loadProducts();

    } catch (err) {
        showToast(`❌ Checkout failed: ${err.message}`, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'PROCESS CHECKOUT';
    }
}

// ── UI helpers ──────────────────────────────────────────────
function checkLowStockAlerts() {
    const lowCount = allProducts.filter(p => !p.isService && p.stock <= (p.minStock || 5)).length;
    const badge = document.getElementById('outOfStockBadge');
    if (badge) {
        badge.innerText = lowCount;
        badge.classList.toggle('hidden', lowCount === 0);
    }
}

function updateCartUI() {
    const cartContainer = document.getElementById('cartItems');
    let subtotal = 0;

    if (cart.length === 0) {
        cartContainer.innerHTML = '<div class="empty-cart-state">Cart is empty</div>';
    } else {
        cartContainer.innerHTML = '';
        cart.forEach(item => {
            const itemTotal = item.price * item.qty;
            subtotal += itemTotal;

            const div = document.createElement('div');
            div.className = 'cart-item';
            div.innerHTML = `
                <div class="item-desc">
                    <h4>${item.name}</h4>
                    <p>$${itemTotal.toFixed(2)}</p>
                </div>
                <div class="item-qty">
                    <button class="qty-btn" onclick="updateCartQty(${item.id}, -1)">-</button>
                    <span>${item.qty}</span>
                    <button class="qty-btn" onclick="updateCartQty(${item.id}, 1)">+</button>
                </div>
            `;
            cartContainer.appendChild(div);
        });
    }

    const tax   = subtotal * 0.10;
    const total = subtotal + tax;
    document.getElementById('subTotal').innerText  = `$${subtotal.toFixed(2)}`;
    document.getElementById('taxTotal').innerText  = `$${tax.toFixed(2)}`;
    document.getElementById('grandTotal').innerText = `$${total.toFixed(2)}`;
}

function showToast(msg, type = 'info') {
    let toast = document.getElementById('posToast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'posToast';
        toast.style.cssText = `
            position:fixed; bottom:24px; left:50%; transform:translateX(-50%);
            padding:14px 28px; border-radius:12px; font-weight:700; font-size:1rem;
            color:#fff; z-index:9999; transition:opacity .3s; white-space:nowrap;`;
        document.body.appendChild(toast);
    }
    const colors = { success: '#10b981', error: '#ef4444', warn: '#f59e0b', info: '#3b82f6' };
    toast.style.background = colors[type] || colors.info;
    toast.style.opacity = '1';
    toast.textContent = msg;
    setTimeout(() => { toast.style.opacity = '0'; }, 4000);
}
