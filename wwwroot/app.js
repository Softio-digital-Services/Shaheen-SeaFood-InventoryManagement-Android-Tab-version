// ─── INTERNATIONALIZATION (i18n) ENGINE ───────────────────────────────────
const TRANSLATIONS = {
    en: {
        // Login
        login_title: 'Inventory Portal',
        login_subtitle: 'Sign in to manage your inventory',
        login_username: 'Username',
        login_password: 'Password',
        login_btn: 'Sign In',
        // Lock screen
        lock_unlock: 'Unlock',
        lock_switch: 'Switch User / Logout',
        lock_title: 'Session Locked',
        lock_subtitle: 'Enter password to resume',
        // Order / Cart
        order_title: 'Order',
        cart_clear: 'Clear',
        cart_currency: 'Display Currency',
        cart_subtotal: 'Subtotal',
        cart_tax: 'Tax (10%)',
        cart_total: 'Total',
        cart_checkout: 'Process Checkout',
        // Scanner status
        scanner_ready: 'Scanner Ready',
        // Notifications
        notif_header: 'System Alerts',
        notif_all_good: 'All good!',
        notif_low_stock: 'Low Stock',
        notif_out_of_stock: 'Out of Stock',
        // Products
        all_parts: 'All Parts',
        stock_label: 'Stock',
        // Add/Edit Modal
        modal_add_title: 'Add New Product',
        modal_edit_title: 'Edit Product',
        modal_add_btn: 'Add Product',
        modal_save_btn: 'Save Changes',
        modal_field_name: 'Product Name',
        modal_field_name_ph: 'e.g. Salmon Fillet',
        modal_field_category: 'Category',
        modal_field_price: 'Price ($)',
        modal_field_stock: 'Initial Stock',
        modal_field_barcode: 'Barcode / SKU',
        modal_field_barcode_ph: 'Scan or type barcode',
        // Returns modal
        returns_title: 'Order Returns',
        returns_reason_label: 'Reason for Return',
        returns_reason_ph: 'e.g. Defective item',
        returns_back: 'Back',
        returns_process: 'Process Refund',
        // Camera
        camera_flip: 'Flip Camera',
    },
    ar: {
        // Login
        login_title: 'بوابة المخزون',
        login_subtitle: 'سجّل دخولك لإدارة المستودع',
        login_username: 'اسم المستخدم',
        login_password: 'كلمة المرور',
        login_btn: 'تسجيل الدخول',
        // Lock screen
        lock_unlock: 'إلغاء القفل',
        lock_switch: 'تبديل المستخدم / تسجيل الخروج',
        lock_title: 'الجلسة مقفلة',
        lock_subtitle: 'أدخل كلمة المرور للمتابعة',
        // Order / Cart
        order_title: 'الطلب',
        cart_clear: 'مسح',
        cart_currency: 'عملة العرض',
        cart_subtotal: 'المجموع الفرعي',
        cart_tax: 'الضريبة (10%)',
        cart_total: 'الإجمالي',
        cart_checkout: 'إتمام الدفع',
        // Scanner status
        scanner_ready: 'الماسح جاهز',
        // Notifications
        notif_header: 'تنبيهات النظام',
        notif_all_good: 'كل شيء على ما يرام!',
        notif_low_stock: 'مخزون منخفض',
        notif_out_of_stock: 'نفد المخزون',
        // Products
        all_parts: 'جميع القطع',
        stock_label: 'المخزون',
        // Add/Edit Modal
        modal_add_title: 'إضافة منتج جديد',
        modal_edit_title: 'تعديل المنتج',
        modal_add_btn: 'إضافة المنتج',
        modal_save_btn: 'حفظ التغييرات',
        modal_field_name: 'اسم المنتج',
        modal_field_name_ph: 'مثال: فيليه السلمون',
        modal_field_category: 'الفئة',
        modal_field_price: 'السعر',
        modal_field_stock: 'الكمية الابتدائية',
        modal_field_barcode: 'الباركود / الرمز',
        modal_field_barcode_ph: 'امسح أو اكتب الباركود',
        // Returns modal
        returns_title: 'مرتجعات الطلبات',
        returns_reason_label: 'سبب الإرجاع',
        returns_reason_ph: 'مثال: منتج معيب',
        returns_back: 'رجوع',
        returns_process: 'معالجة الاسترداد',
        // Camera
        camera_flip: 'تبديل الكاميرا',
    }
};

let currentLang = localStorage.getItem('pos_lang') || 'en';

function t(key) {
    return (TRANSLATIONS[currentLang] && TRANSLATIONS[currentLang][key]) || TRANSLATIONS['en'][key] || key;
}

function applyLanguage() {
    const isRtl = currentLang === 'ar';
    const html = document.documentElement;

    // Set html attributes
    html.setAttribute('lang', currentLang);
    html.setAttribute('dir', isRtl ? 'rtl' : 'ltr');

    // Update lang toggle button label
    const langLabel = document.getElementById('langLabel');
    if (langLabel) langLabel.textContent = isRtl ? 'EN' : 'AR';

    // Translate all static data-i18n elements
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });

    // Translate dynamic placeholders
    const searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.placeholder = isRtl ? 'ابحث في المخزون...' : 'Search inventory...';

    // Add/Edit modal input placeholders
    const newItemName = document.getElementById('newItemName');
    if (newItemName) newItemName.placeholder = t('modal_field_name_ph');

    const newItemBarcode = document.getElementById('newItemBarcode');
    if (newItemBarcode) newItemBarcode.placeholder = t('modal_field_barcode_ph');

    // Returns modal placeholder
    const returnReason = document.getElementById('returnReason');
    if (returnReason) returnReason.placeholder = t('returns_reason_ph');

    // Login screen placeholders
    const loginUser = document.getElementById('loginUser');
    if (loginUser) loginUser.placeholder = isRtl ? 'مدير' : 'admin';

    // Camera modal flip button text
    const btnSwitch = document.getElementById('btnSwitchCamera');
    if (btnSwitch) {
        // Preserve the svg, update the text node
        const textNode = [...btnSwitch.childNodes].find(n => n.nodeType === Node.TEXT_NODE);
        if (textNode) textNode.textContent = '\n                    ' + t('camera_flip') + '\n                ';
    }

    const scannerStatus = document.getElementById('scannerStatus');
    if (scannerStatus) {
        // Preserve the SVG icon, only update the trailing text node
        const textNode = [...scannerStatus.childNodes].find(n => n.nodeType === Node.TEXT_NODE);
        if (textNode) textNode.textContent = '\n                        ' + t('scanner_ready') + '\n                    ';
    }

    // Re-render dynamic content so it picks up new language
    renderCategories();
    renderProducts();
    checkLowStockAlerts();

    // Persist preference
    localStorage.setItem('pos_lang', currentLang);
}

// ─── END i18n ENGINE ──────────────────────────────────────────────────────

// STATE MANAGEMENT (Now dynamic with fallbacks)
const DEFAULT_PRODUCTS = [
    { id: 1, name: 'Fresh Salmon Fillet', price: 18.50, stock: 40, category: 'Seafood', image: '🐟' },
    { id: 2, name: 'Garlic Butter', price: 4.25, stock: 15, category: 'Groceries', image: '🧈' },
    { id: 3, name: 'White Onion', price: 1.50, stock: 80, category: 'Vegetables', image: '🧅' },
    { id: 10, name: 'Full Kitchen Prep Service', price: 50.00, stock: 999, category: 'Services', image: '🍽️', isService: true }
];

// Mock fetch for local browser testing/verification
if (typeof window !== 'undefined' && !window.AndroidBridge) {
    console.log("Mocking fetch API for offline browser testing");
    const mockDb = {
        products: [
            { id: 1, name: 'Fresh Salmon Fillet', price: 18.50, stock: 40, category: 'Seafood', image: '🐟' },
            { id: 2, name: 'Garlic Butter', price: 4.25, stock: 15, category: 'Groceries', image: '🧈' },
            { id: 3, name: 'White Onion', price: 1.50, stock: 80, category: 'Vegetables', image: '🧅' },
            { id: 4, name: 'Tomato Paste', price: 2.99, stock: 50, category: 'Groceries', image: '🥫' }
        ],
        categories: ['Seafood', 'Groceries', 'Vegetables', 'Services'],
        recipes: [
            {
                id: 1,
                name: 'Garlic Butter Salmon',
                description: 'Salmon fillet cooked in fresh butter sauce',
                price: 24.99,
                totalCost: 11.25,
                parts: [
                    { partId: 4, partName: 'Fresh Salmon Fillet', qty: 1.0, unitCost: 10.00 },
                    { partId: 5, partName: 'Garlic Butter', qty: 0.5, unitCost: 2.50 }
                ]
            }
        ],
        reports: {
            revenue: 1250.50,
            orders: 45,
            outOfStock: 2,
            lowStock: 4,
            categorySales: [
                { category: 'Seafood', sales: 750.00 },
                { category: 'Engine', sales: 300.00 },
                { category: 'Brakes', sales: 200.50 }
            ],
            transactions: [
                { action: 'SALE', item: 'POS Sale', desc: 'Order #101 -- Total: $24.99', user: 'Admin', time: '2026-07-02 14:15' },
                { action: 'STOCK_ADD', item: 'Fresh Salmon Fillet', desc: 'Added via Import (Qty: 50)', user: 'Admin', time: '2026-07-02 13:00' }
            ]
        }
    };

    window.fetch = async (url, options = {}) => {
        const path = url.replace(/https?:\/\/[^\/]+/, '').replace(/^\//, '');
        console.log("Mock Fetch Request:", path, options);
        
        let responseData = {};
        let status = 200;

        if (path === 'api/config') {
            responseData = { language: 'en', isArabic: false, primaryColor: '#0ea5e9', primaryRgb: '14, 165, 233' };
        } else if (path === 'api/login') {
            responseData = { username: 'Softio.Admin', fullName: 'Softio Admin' };
        } else if (path === 'api/products') {
            responseData = mockDb.products;
        } else if (path === 'api/categories') {
            responseData = mockDb.categories;
        } else if (path === 'api/currencies') {
            responseData = [{ code: 'USD', symbol: '$', rate: 1 }];
        } else if (path === 'api/recipes') {
            if (options.method === 'POST') {
                const body = JSON.parse(options.body);
                if (body.id) {
                    const r = mockDb.recipes.find(x => x.id === parseInt(body.id));
                    if (r) Object.assign(r, body);
                } else {
                    body.id = mockDb.recipes.length + 1;
                    mockDb.recipes.push(body);
                }
            }
            responseData = mockDb.recipes;
        } else if (path.startsWith('api/recipes/') && options.method === 'DELETE') {
            const id = parseInt(path.split('/').pop());
            mockDb.recipes = mockDb.recipes.filter(r => r.id !== id);
            responseData = { success: true };
        } else if (path.startsWith('api/products/') && options.method === 'DELETE') {
            const id = parseInt(path.split('/').pop());
            mockDb.products = mockDb.products.filter(p => p.id !== id);
            responseData = { success: true };
        } else if (path === 'api/add-item') {
            const body = JSON.parse(options.body);
            if (body.id) {
                const p = mockDb.products.find(x => x.id === parseInt(body.id));
                if (p) Object.assign(p, body);
            } else {
                body.id = mockDb.products.length + 1;
                mockDb.products.push(body);
            }
            responseData = { success: true };
        } else if (path === 'api/checkout') {
            responseData = { success: true, orderId: 102, total: 45.00 };
        } else if (path === 'api/import-items') {
            const body = JSON.parse(options.body);
            body.items.forEach((item, idx) => {
                mockDb.products.push({
                    id: mockDb.products.length + 1,
                    name: item.name,
                    price: item.price,
                    stock: item.stock,
                    category: item.category,
                    barcode: item.barcode,
                    sku: item.sku
                });
            });
            responseData = { success: true, imported: body.items.length, skipped: 0 };
        } else if (path === 'api/import-sales') {
            const body = JSON.parse(options.body);
            let processed = 0;
            let skipped = 0;
            const skippedNames = [];
            const deductions = [];

            body.sales.forEach(sale => {
                const cleanName = sale.recipeName.toLowerCase().replace(/[\s\t\r\n]/g, '');
                const recipe = mockDb.recipes.find(r => r.name.toLowerCase().replace(/[\s\t\r\n]/g, '') === cleanName);
                if (!recipe) {
                    skipped++;
                    skippedNames.push(sale.recipeName);
                    return;
                }

                recipe.parts.forEach(part => {
                    const prod = mockDb.products.find(p => p.id === part.partId);
                    if (prod) {
                        const qtyDeducted = part.qty * sale.qtySold;
                        if (qtyDeducted <= 0) return;
                        
                        const prevStock = prod.stock;
                        prod.stock = parseFloat((prod.stock - qtyDeducted).toFixed(2));

                        const existingDeduct = deductions.find(d => d.partId === part.partId);
                        if (existingDeduct) {
                            existingDeduct.qtyDeducted = parseFloat((existingDeduct.qtyDeducted + qtyDeducted).toFixed(2));
                            existingDeduct.newStock = prod.stock;
                        } else {
                            deductions.push({
                                partId: part.partId,
                                partName: prod.name,
                                qtyDeducted: qtyDeducted,
                                previousStock: prevStock,
                                newStock: prod.stock
                            });
                        }

                        // Log transaction
                        mockDb.reports.transactions.unshift({
                            action: 'STOCK_DEDUCT',
                            item: prod.name,
                            desc: `Deducted ${qtyDeducted.toFixed(2).replace(/\.00$/, '')} via sales import (${recipe.name} x${sale.qtySold})`,
                            user: 'Admin',
                            time: new Date().toISOString().replace('T', ' ').slice(0, 16)
                        });
                    }
                });
                processed++;
            });

            responseData = { success: true, processed, skipped, skippedNames, deductions };
        } else if (path === 'api/export-csv') {
            responseData = { success: true, path: 'Downloads/mock_export.csv' };
        } else if (path === 'api/reports') {
            responseData = mockDb.reports;
        }

        return {
            ok: status >= 200 && status < 300,
            status: status,
            json: async () => responseData,
            text: async () => JSON.stringify(responseData)
        };
    };
}

let allProducts = [...DEFAULT_PRODUCTS];
let cart = [];
let currentCategory = 'All';
let currencies = [];
let currentCurrency = { code: 'USD', symbol: '$', rate: 1 };
const API_BASE = ''; 

// CORE INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    // Layout Mode Detection for Mobile & Android devices
    const checkLayoutMode = () => {
        const isMobile = window.innerWidth <= 1024 || !!window.AndroidBridge;
        if (isMobile) {
            document.body.classList.add('is-mobile-layout');
        } else {
            document.body.classList.remove('is-mobile-layout');
        }
    };
    checkLayoutMode();
    window.addEventListener('resize', checkLayoutMode);

    // 0. Fetch backend language config immediately to sync web portal language with desktop app natively before UI renders
    await fetchLanguageConfig();
    applyLanguage();

    // 1. Setup UI Handlers First (Ensures buttons work immediately)
    checkLoginState(); 
    


    // MODAL HANDLERS
    safeListen('btnOpenAddModal', 'click', () => {
        document.getElementById('modalTitle').innerText = t('modal_add_title');
        document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
        document.getElementById('editItemId').value = '';
        document.getElementById('addItemModal').classList.remove('hidden');
    });

    safeListen('btnCloseAddModal', 'click', () => {
        document.getElementById('addItemModal').classList.add('hidden');
    });

    safeListen('btnFullscreen', 'click', () => {
        const enterIcon = document.getElementById('fsIconEnter');
        const exitIcon = document.getElementById('fsIconExit');
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen().then(() => {
                enterIcon?.classList.add('hidden');
                exitIcon?.classList.remove('hidden');
            }).catch(() => showToast("Fullscreen restricted", "warn"));
        } else {
            document.exitFullscreen().then(() => {
                enterIcon?.classList.remove('hidden');
                exitIcon?.classList.add('hidden');
            });
        }
    });

    document.addEventListener('fullscreenchange', () => {
        const isFS = !!document.fullscreenElement;
        document.getElementById('fsIconEnter')?.classList.toggle('hidden', isFS);
        document.getElementById('fsIconExit')?.classList.toggle('hidden', !isFS);
    });

    safeListen('btnNotifications', 'click', (e) => {
        e.stopPropagation();
        document.getElementById('notificationPanel').classList.toggle('hidden');
    });

    document.addEventListener('click', () => {
        const panel = document.getElementById('notificationPanel');
        if (panel) panel.classList.add('hidden');
    });

    safeListen('btnCloseScanner', 'click', () => barcodeScannerManager.stop());
    safeListen('btnSubmitItem', 'click', submitNewItem);
    safeListen('btnCameraScan', 'click', () => barcodeScannerManager.start('search'));
    safeListen('btnModalCameraScan', 'click', () => barcodeScannerManager.start('modal'));
    safeListen('btnSwitchCamera', 'click', () => barcodeScannerManager.toggleCamera());
    safeListen('btnClearCart', 'click', clearCart);
    safeListen('btnCheckout', 'click', processCheckout);
    safeListen('btnLogin', 'click', handleLogin);
    safeListen('btnLock', 'click', handleLock);
    safeListen('btnUnlock', 'click', handleUnlock);
    safeListen('btnLockLogout', 'click', handleLogout);
    safeListen('btnLogout', 'click', handleLogout); 
    safeListen('btnOpenReturns', 'click', openReturnsModal);
    safeListen('btnCloseReturnsModal', 'click', () => document.getElementById('returnsModal').classList.add('hidden'));
    safeListen('btnProcessReturn', 'click', processReturn);
    
    safeListen('currencySelect', 'change', (e) => {
        const code = e.target.value;
        const curr = currencies.find(c => c.code === code);
        if (curr) {
            currentCurrency = curr;
            renderProducts();
            updateCartUI();
        }
    });    
    safeListen('lockPass', 'keydown', (e) => {
        if (e.key === 'Enter') handleUnlock();
    });    
    // PASSWORD TOGGLE
    const toggle = document.getElementById('togglePassword');
    const passIn = document.getElementById('loginPass');
    if (toggle && passIn) {
        toggle.onclick = () => {
            const isShowing = passIn.type === 'text';
            passIn.type = isShowing ? 'password' : 'text';
            document.getElementById('eyeOpen').classList.toggle('hidden', isShowing);
            document.getElementById('eyeClosed').classList.toggle('hidden', !isShowing);
            toggle.classList.toggle('active-green', !isShowing);
        };
        
        // Login on Enter key
        passIn.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') handleLogin();
        });
        const userIn = document.getElementById('loginUser');
        if (userIn) {
            userIn.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') handleLogin();
            });
        }
    }

    // SEARCH HANDLER
    safeListen('searchInput', 'input', () => {
        renderProducts();
    });

    setupNotificationSystem();
    setupSignalR(); 

    // 2. Load Data in Background
    initApp().then(() => {
        switchTab('inventory');
    });
});

// UTILITIES
function safeListen(id, event, callback) {
    const el = document.getElementById(id);
    if (el) el.addEventListener(event, callback);
}

async function fetchLanguageConfig() {
    try {
        const res = await fetch(`${API_BASE}/api/config`);
        if (res.ok) {
            const data = await res.json();
            if (data.language && data.language !== currentLang) {
                currentLang = data.language;
                applyLanguage();
            }
            if (data.primaryColor) {
                document.documentElement.style.setProperty('--accent', data.primaryColor);
                document.documentElement.style.setProperty('--accent-hover', data.primaryColor);
            }
            if (data.primaryRgb) {
                document.documentElement.style.setProperty('--accent-rgb', data.primaryRgb);
            }
        }
    } catch (e) { console.error("Language config fetch failed", e); }
}

let allRecipes = [];

function refreshActiveTab() {
    const activeTab = document.querySelector('.nav-tab.active');
    if (activeTab) {
        if (activeTab.id === 'tabBtnInventory') {
            loadInventoryTable();
        } else if (activeTab.id === 'tabBtnRecipes') {
            loadRecipesTab();
        } else if (activeTab.id === 'tabBtnReports') {
            loadReportsData();
        }
    }
}

async function initApp() {
    try {
        await fetchLanguageConfig();
        await fetchInventory();
        await fetchCategories();
        await fetchCurrencies();
        await fetchRecipes();
        checkLowStockAlerts();
    } catch (e) {
        console.error("Init failed", e);
    }
    refreshActiveTab();
}

async function fetchCurrencies() {
    try {
        const res = await fetch(`${API_BASE}/api/currencies`);
        if (res.ok) {
            currencies = await res.json();
            const select = document.getElementById('currencySelect');
            if (select) {
                select.innerHTML = currencies.map(c => `<option value="${c.code}" ${c.code === currentCurrency.code ? 'selected' : ''}>${c.code} (${c.symbol})</option>`).join('');
            }
        }
    } catch (e) { console.error("Currencies failed", e); }
}

function formatPrice(usdPrice) {
    const converted = usdPrice * currentCurrency.rate;
    return `${currentCurrency.symbol}${converted.toFixed(2)}`;
}

async function fetchInventory() {
    try {
        const res = await fetch(`${API_BASE}/api/products`);
        if (res.ok) {
            allProducts = await res.json();
        } else {
            throw new Error("API responded with error");
        }
    } catch (err) {
        console.error("Fetch inventory failed, using fallbacks", err);
        allProducts = [...DEFAULT_PRODUCTS];
        showToast("Offline Mode: Local data loaded", "warn");
    }
}

async function fetchCategories() {
    try {
        const res = await fetch(`${API_BASE}/api/categories`);
        if (res.ok) {
            const cats = await res.json();
            renderCategories(cats);
        }
    } catch (err) {
        renderCategories([]);
    }
}

let masterCategories = [];

function renderCategories(apiCategories = null) {
    const container = document.getElementById('categoryList');
    if (!container) return;
    
    // If we received new categories from API, update our master list
    if (apiCategories) {
        masterCategories = apiCategories;
    }

    const categories = ['All', ...masterCategories, 'Recipes'];
    container.innerHTML = '';
    
    // Sync the Add Item modal category dropdown
    const modalSelect = document.getElementById('newItemCategory');
    if (modalSelect) {
        modalSelect.innerHTML = '';
        masterCategories.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat;
            opt.innerText = cat;
            modalSelect.appendChild(opt);
        });
    }
    
    categories.forEach(cat => {
        const btn = document.createElement('button');
        btn.className = `cat-btn ${currentCategory === cat ? 'active' : ''}`;
        btn.innerText = cat === 'All' ? t('all_parts') : cat;
        btn.onclick = () => {
            currentCategory = cat;
            renderCategories();
            renderProducts();
        };
        container.appendChild(btn);
    });
}

function renderProducts() {
    const grid = document.getElementById('productGrid');
    if (!grid) return;
    grid.innerHTML = '';

    const query = document.getElementById('searchInput')?.value.toLowerCase() || '';

    if (currentCategory === 'Recipes') {
        const filtered = allRecipes.filter(r => {
            return !query || (r.name && r.name.toLowerCase().includes(query));
        });

        filtered.forEach(r => {
            const card = document.createElement('div');
            card.className = 'product-card recipe-product-card';
            card.onclick = () => addRecipeToCart(r);
            card.innerHTML = `
                <div class="product-img"><span class="emoji-icon">🍲</span></div>
                <div class="product-info">
                    <div class="product-name">${r.name}</div>
                    <div class="product-price">${formatPrice(r.price)}</div>
                    <div class="product-stock" style="color:var(--accent);">Recipe</div>
                </div>
            `;
            grid.appendChild(card);
        });
        return;
    }

    const filtered = allProducts.filter(p => {
        const matchesCategory = currentCategory === 'All' || p.category === currentCategory;
        const matchesSearch = !query || 
            (p.name && p.name.toLowerCase().includes(query)) || 
            (p.sku && p.sku.toLowerCase().includes(query)) || 
            (p.barcode && p.barcode.toLowerCase().includes(query));
        return matchesCategory && matchesSearch;
    });

    filtered.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.onclick = () => addToCart(p);
        
        // Priority: Item Image -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage && itemImage.length > 5 && itemImage.includes('/')) {
            // It's a path to a product image
            displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
        } else if (catImage && catImage.length > 5 && catImage.includes('/')) {
            // It's a path to a category icon
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            // Fallback to emoji or default box
            displayContent = `<span class="emoji-icon">${itemImage || '📦'}</span>`;
        }

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${p.name}</div>
                <div class="product-price">${formatPrice(p.price)}</div>
                <div class="product-stock ${p.stock < 5 ? 'low' : ''}">${t('stock_label')}: ${p.stock}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openEditModal(id) {
    const item = allProducts.find(p => p.id === id);
    if (!item) return;
    document.getElementById('modalTitle').innerText = t('modal_edit_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_save_btn');
    document.getElementById('editItemId').value = item.id;
    document.getElementById('newItemName').value = item.name;
    document.getElementById('newItemCategory').value = item.category || (masterCategories.length > 0 ? masterCategories[0] : '');
    document.getElementById('newItemPrice').value = item.price;
    document.getElementById('newItemStock').value = item.stock;
    document.getElementById('newItemBarcode').value = item.barcode || '';
    document.getElementById('addItemModal').classList.remove('hidden');
}

async function submitNewItem() {
    const editId = document.getElementById('editItemId').value;
    const parsedId = editId ? parseInt(editId) : null;
    const itemData = {
        name: document.getElementById('newItemName').value,
        category: document.getElementById('newItemCategory').value,
        price: parseFloat(document.getElementById('newItemPrice').value),
        stock: parseInt(document.getElementById('newItemStock').value),
        barcode: document.getElementById('newItemBarcode').value
    };

    if (!itemData.name || isNaN(itemData.price)) {
        showToast("Please fill Name and Price", "error");
        return;
    }

    try {
        const res = await fetch(`${API_BASE}/api/add-item`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ...itemData, id: parsedId })
        });

        if (res.ok) {
            showToast(editId ? "Item updated!" : "Item added!", "success");
            await initApp(); // Reload everything
            document.getElementById('addItemModal').classList.add('hidden');
        } else {
            const err = await res.json();
            showToast(err.error || "Failed to save", "error");
        }
    } catch (err) {
        showToast("Connection error", "error");
    }
}

function addToCart(product) {
    if (product.stock <= 0) {
        showToast("Out of stock!", "warn");
        return;
    }
    const existing = cart.find(item => item.id === product.id && item.itemType === 'Part');
    if (existing) {
        existing.quantity++;
    } else {
        cart.push({ ...product, quantity: 1, itemType: 'Part' });
    }
    updateCartUI();
}

function addRecipeToCart(recipe) {
    const existing = cart.find(item => item.id === recipe.id && item.itemType === 'Recipe');
    if (existing) {
        existing.quantity++;
    } else {
        cart.push({
            id: recipe.id,
            name: recipe.name,
            price: recipe.price,
            quantity: 1,
            itemType: 'Recipe',
            recipeId: recipe.id,
            image: '🍲'
        });
    }
    updateCartUI();
}

function updateCartUI() {
    const list = document.getElementById('cartItems');
    if (!list) return;
    list.innerHTML = '';
    let subtotal = 0;

    cart.forEach(item => {
        const total = item.price * item.quantity;
        subtotal += total;
        const div = document.createElement('div');
        div.className = 'cart-item';
        div.innerHTML = `
            <div class="cart-item-info">
                <div class="cart-item-name">${item.name}</div>
                <div class="cart-item-price">${formatPrice(item.price)}</div>
            </div>
            <div class="qty-controls">
                <button class="qty-btn" onclick="changeQty(${item.id}, -1, '${item.itemType || 'Part'}')">-</button>
                <div class="qty-val">${item.quantity}</div>
                <button class="qty-btn" onclick="changeQty(${item.id}, 1, '${item.itemType || 'Part'}')">+</button>
            </div>
            <div class="cart-item-total">${formatPrice(total)}</div>
        `;
        list.appendChild(div);
    });

    const taxRate = document.getElementById('applyTax')?.checked ? 0.10 : 0;
    const tax = subtotal * taxRate;
    document.getElementById('subTotal').innerText = formatPrice(subtotal);
    document.getElementById('taxTotal').innerText = formatPrice(tax);
    document.getElementById('grandTotal').innerText = formatPrice(subtotal + tax);

    // Update floating cart badge count
    const totalQty = cart.reduce((sum, item) => sum + item.quantity, 0);
    const floatCount = document.getElementById('cartFloatingCount');
    if (floatCount) {
        floatCount.innerText = totalQty;
        const btn = document.getElementById('floatingCartBtn');
        if (btn) {
            if (totalQty > 0) {
                btn.classList.add('visible');
            } else {
                btn.classList.remove('visible');
                const sidebar = document.querySelector('.cart-sidebar');
                if (sidebar && sidebar.classList.contains('open')) {
                    toggleCartDrawer();
                }
            }
        }
    }
}

function changeQty(id, delta, itemType = 'Part') {
    const item = cart.find(x => x.id === id && x.itemType === itemType);
    if (!item) return;

    if (item.quantity + delta <= 0) {
        cart = cart.filter(x => !(x.id === id && x.itemType === itemType));
    } else {
        if (itemType === 'Part') {
            const product = allProducts.find(p => p.id === id);
            if (delta > 0 && product && item.quantity >= product.stock && !product.isService) {
                showToast("No more stock available", "warn");
                return;
            }
        }
        item.quantity += delta;
    }
    updateCartUI();
}

function clearCart() { cart = []; updateCartUI(); }

async function processCheckout() {
    if (cart.length === 0) return;
    
    try {
        const payloadItems = cart.map(i => ({
            id: i.itemType === 'Recipe' ? 0 : i.id,
            name: i.name,
            qty: i.quantity,
            price: i.price,
            itemType: i.itemType || 'Part',
            recipeId: i.itemType === 'Recipe' ? i.id : null
        }));

        const res = await fetch(`${API_BASE}/api/checkout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: payloadItems })
        });

        if (res.ok) {
            cart = [];
            updateCartUI();
            await fetchInventory();
            await fetchRecipes();
            renderProducts();
            showToast("Transaction Complete!", "success");
        } else {
            const err = await res.json();
            showToast(err.error || "Checkout failed", "error");
        }
    } catch (err) {
        showToast("Connection error", "error");
    }
}

// LOGIN SYSTEM
function checkLoginState() {
    // ALWAYS clear session on refresh as per user requirement for tablet POS security
    localStorage.removeItem('pos_loggedIn');
    document.getElementById('loginScreen').classList.remove('hidden');
    // Hide header on login page
    const topNav = document.querySelector('.top-nav');
    if (topNav) topNav.style.display = 'none';
}

async function handleLogin() {
    const user = document.getElementById('loginUser').value;
    const pass = document.getElementById('loginPass').value;
    const errorBox = document.getElementById('loginError');

    if (errorBox) {
        errorBox.classList.add('hidden');
        errorBox.innerText = '';
    }

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            const data = await res.json();
            localStorage.setItem('pos_loggedIn', 'true');
            localStorage.setItem('pos_username', data.username);
            localStorage.setItem('pos_user', data.fullName);
            document.getElementById('loginScreen').classList.add('hidden');
            // Show header after successful login
            const topNav = document.querySelector('.top-nav');
            if (topNav) topNav.style.display = 'flex';
            showToast(`Logged in as ${data.fullName}`, "success");
            globalBarcodeScanner.init(); // Activate scanner immediately on login
            initApp().then(() => {
                switchTab('inventory');
            });
        } else {
            const msg = "Invalid username or password";
            if (errorBox) {
                errorBox.innerText = msg;
                errorBox.classList.remove('hidden');
            }
            showToast(msg, "error");
        }
    } catch (err) {
        const msg = "Login server offline";
        if (errorBox) {
            errorBox.innerText = msg;
            errorBox.classList.remove('hidden');
        }
        showToast(msg, "error");
    }
}

function handleLogout() {
    localStorage.removeItem('pos_loggedIn');
    localStorage.removeItem('pos_username');
    localStorage.removeItem('pos_user');
    document.getElementById('lockScreen').classList.add('hidden');
    document.getElementById('loginScreen').classList.remove('hidden');
    showToast("Logged out", "info");
}

function handleLock() {
    const user = localStorage.getItem('pos_user') || 'User';
    document.getElementById('lockUserDisplay').innerText = user;
    document.getElementById('lockPass').value = '';
    document.getElementById('lockScreen').classList.remove('hidden');
}

async function handleUnlock() {
    const user = localStorage.getItem('pos_username');
    const pass = document.getElementById('lockPass').value;
    
    if (!user) { handleLogout(); return; }

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            document.getElementById('lockScreen').classList.add('hidden');
            showToast("Session Unlocked", "success");
        } else {
            showToast("Invalid password", "error");
        }
    } catch (err) {
        showToast("Server offline", "error");
    }
}

// ── GLOBAL HID BARCODE SCANNER ──────────────────────────────────────────────
// USB/Bluetooth scanners act as keyboards: they type chars very fast then Enter.
// We detect this by checking the time between keystrokes (< 50ms = scanner).
const globalBarcodeScanner = {
    buffer: '',
    lastKeyTime: 0,
    THRESHOLD_MS: 50,   // Max ms between scanner keystrokes
    MIN_LENGTH: 3,      // Minimum barcode length to process

    init() {
        document.addEventListener('keydown', (e) => this.onKey(e));
        console.log('Global barcode scanner ready.');
    },

    onKey(e) {
        // Ignore if user is focused on an input/textarea/select
        const tag = document.activeElement?.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

        // Ignore if login screen is visible
        const loginScreen = document.getElementById('loginScreen');
        if (loginScreen && !loginScreen.classList.contains('hidden')) return;

        const lockScreen = document.getElementById('lockScreen');
        if (lockScreen && !lockScreen.classList.contains('hidden')) return;

        // Ignore if a modal is open
        const modal = document.getElementById('addItemModal');
        if (modal && !modal.classList.contains('hidden')) return;

        const now = Date.now();
        const timeSinceLast = now - this.lastKeyTime;
        this.lastKeyTime = now;

        if (e.key === 'Enter') {
            const code = this.buffer.trim();
            this.buffer = '';
            if (code.length >= this.MIN_LENGTH) {
                e.preventDefault();
                e.stopPropagation();
                this.processBarcode(code);
                // Force blur any focused button to prevent accidental re-triggers
                if (document.activeElement instanceof HTMLElement) {
                    document.activeElement.blur();
                }
            }
            return;
        }

        // If too much time has passed, this is a new sequence — reset buffer
        if (timeSinceLast > 500) this.buffer = '';

        // Only accumulate printable single characters
        if (e.key.length === 1) this.buffer += e.key;
    },

    processBarcode(code) {
        // Search by barcode first, then by SKU
        const product = allProducts.find(p => p.barcode === code) 
                     || allProducts.find(p => p.sku === code);

        if (product) {
            addToCart(product);
            showToast(`✅ Added: ${product.name}`, 'success');
        } else {
            showToast(`⚠️ Barcode not found: ${code}`, 'error');
        }
    }
};

function checkLowStockAlerts() {
    const lowItems = allProducts.filter(p => p.stock <= (p.minStock || 5));
    const badge = document.getElementById('outOfStockBadge');
    const list = document.getElementById('notificationList');
    if (badge) {
        badge.innerText = lowItems.length;
        badge.classList.toggle('hidden', lowItems.length === 0);
    }
    if (list) {
        list.innerHTML = lowItems.length === 0 
            ? `<div class="notif-item"><div class="title">${t('notif_all_good')}</div></div>` 
            : '';
        lowItems.forEach(item => {
            const div = document.createElement('div');
            const isOut = item.stock <= 0;
            div.className = `notif-item ${isOut ? 'out-of-stock' : 'low-stock'}`;
            div.innerHTML = `<div class="title">${item.name}</div><div class="desc">${isOut ? t('notif_out_of_stock') : t('notif_low_stock')}: ${item.stock} left</div>`;
            list.appendChild(div);
        });
    }
}

function setupNotificationSystem() {
    const taxToggle = document.getElementById('applyTax');
    if (taxToggle) taxToggle.onchange = updateCartUI;

    // Periodic check for low stock (every 30 seconds, like desktop)
    checkLowStockAlerts();
    setInterval(checkLowStockAlerts, 30000);

    // Ensure the panel can be closed by clicking outside
    document.addEventListener('click', (e) => {
        const panel = document.getElementById('notificationPanel');
        const bell = document.getElementById('btnNotifications');
        if (panel && !panel.contains(e.target) && !bell.contains(e.target)) {
            panel.classList.add('hidden');
        }
    });
}

function showToast(msg, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.style.cssText = `position:fixed; bottom:30px; left:50%; transform:translateX(-50%); padding:12px 24px; background:rgba(20,20,20,0.95); backdrop-filter:blur(10px); border:1px solid var(--border-color); color:white; border-radius:12px; z-index:999999; font-weight:700; box-shadow:0 10px 30px rgba(0,0,0,0.5);`;
    if (type === 'error') toast.style.borderLeft = '4px solid var(--danger)';
    if (type === 'success') toast.style.borderLeft = '4px solid var(--accent)';
    toast.innerText = msg;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

// SIGNALR REAL-TIME SYNC
async function setupSignalR() {
    if (typeof signalR === 'undefined') {
        console.warn("SignalR library not loaded yet.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/inventory")
        .withAutomaticReconnect()
        .build();

    connection.on("StockUpdated", (reason) => {
        console.log("Real-time sync: StockUpdated", reason);
        fetchInventory().then(() => {
            refreshActiveTab();
            checkLowStockAlerts();
        });
    });

    connection.on("InventoryChanged", (msg) => {
        console.log("Real-time sync: InventoryChanged", msg);
        initApp(); // Full refresh for structural changes
    });

    connection.on("LanguageChanged", (lang) => {
        console.log("Real-time sync: LanguageChanged", lang);
        if (lang && lang !== currentLang) {
            currentLang = lang;
            applyLanguage();
        }
    });

    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error", err);
        setTimeout(setupSignalR, 5000);
    }
}

const barcodeScannerManager = {
    scanner: null,
    target: 'search',
    currentFacingMode: "environment",
    async start(target = 'search') {
        this.target = target;
        
        // Explicitly request permissions first to provide better UX
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: true });
            stream.getTracks().forEach(track => track.stop()); // Stop immediately, just checking permission
        } catch (err) {
            console.error("Camera permission denied", err);
            showToast("Camera access denied. Please enable it in browser settings.", "error");
            return;
        }

        document.getElementById('cameraScannerModal').classList.remove('hidden');
        if (!this.scanner) this.scanner = new Html5Qrcode("reader");
        
        try {
            await this.scanner.start(
                { facingMode: this.currentFacingMode }, 
                { 
                    fps: 24, 
                    qrbox: (w, h) => { 
                        const s = Math.min(w, h) * 0.65; 
                        return { width: s, height: s }; 
                    } 
                }, 
                (t) => this.onScanSuccess(t), 
                () => {}
            );
        } catch (err) { 
            console.error("Scanner start error", err);
            showToast("Failed to start camera. It might be in use.", "error"); 
            document.getElementById('cameraScannerModal').classList.add('hidden');
        }
    },
    async stop() { 
        if (this.scanner && this.scanner.isScanning) { 
            try { await this.scanner.stop(); } catch(e) { console.warn("Scanner stop failed", e); }
        } 
        document.getElementById('cameraScannerModal').classList.add('hidden'); 
    },
    async toggleCamera() { this.currentFacingMode = (this.currentFacingMode === "environment") ? "user" : "environment"; await this.stop(); await this.start(this.target); },
    onScanSuccess(decodedText) {
        if (this.target === 'modal') {
            document.getElementById('newItemBarcode').value = decodedText;
            this.stop(); // Close after scan
        } else {
            const p = allProducts.find(x => x.barcode === decodedText);
            if (p) { 
                addToCart(p); 
                showToast(`Added: ${p.name}`, "success"); 
                this.stop(); // Close after scan
            }
        }
    }
};

// ── RETURNS SYSTEM ────────────────────────────────────────────────────────
let selectedReturnOrder = null;

async function openReturnsModal() {
    document.getElementById('returnsModal').classList.remove('hidden');
    backToOrderList();
    await fetchRecentSales();
}

async function fetchRecentSales() {
    try {
        const res = await fetch(`${API_BASE}/api/recent-sales`);
        if (res.ok) {
            const sales = await res.json();
            const container = document.getElementById('returnsOrderList');
            container.innerHTML = sales.map(s => `
                <div class="order-row" onclick="selectOrderForReturn(${s.orderId})">
                    <div>
                        <div style="font-weight:800; font-size:1rem;">Order #${s.orderId}</div>
                        <div style="font-size:0.8rem; color:var(--text-muted);">${new Date(s.date).toLocaleString()}</div>
                    </div>
                    <div style="text-align:right;">
                        <div style="font-weight:800; color:var(--accent);">${formatPrice(s.total)}</div>
                        <div style="font-size:0.75rem;">${s.customer}</div>
                    </div>
                </div>
            `).join('');
        }
    } catch (e) { showToast("Failed to fetch history", "error"); }
}

async function selectOrderForReturn(orderId) {
    try {
        const res = await fetch(`${API_BASE}/api/order-details/${orderId}`);
        if (res.ok) {
            selectedReturnOrder = { id: orderId, items: await res.json() };
            document.getElementById('returnsOrderList').classList.add('hidden');
            document.getElementById('returnsItemSelection').classList.remove('hidden');
            
            const container = document.getElementById('returnItemsList');
            container.innerHTML = `<h3>Items in Order #${orderId}</h3>` + selectedReturnOrder.items.map(i => `
                <div class="return-item-row">
                    <div style="flex:1;">
                        <div style="font-weight:700;">${i.name}</div>
                        <div style="font-size:0.8rem; color:var(--text-muted);">Purchased: ${i.qty} @ ${formatPrice(i.price)}</div>
                    </div>
                    <div>
                        <label style="font-size:0.6rem; display:block; margin-bottom:2px;">QTY TO RETURN</label>
                        <input type="number" class="return-qty-input" data-pid="${i.partId}" data-price="${i.price}" value="0" min="0" max="${i.qty}">
                    </div>
                </div>
            `).join('');
        }
    } catch (e) { showToast("Failed to load details", "error"); }
}

function backToOrderList() {
    document.getElementById('returnsOrderList').classList.remove('hidden');
    document.getElementById('returnsItemSelection').classList.add('hidden');
    selectedReturnOrder = null;
}

async function processReturn() {
    if (!selectedReturnOrder) return;
    
    const inputs = document.querySelectorAll('.return-qty-input');
    const items = [];
    inputs.forEach(input => {
        const qty = parseInt(input.value);
        if (qty > 0) {
            items.push({
                partId: parseInt(input.dataset.pid),
                qty: qty,
                refundAmount: qty * parseFloat(input.dataset.price)
            });
        }
    });

    if (items.length === 0) {
        showToast("Select at least one item to return", "warn");
        return;
    }

    const payload = {
        orderId: selectedReturnOrder.id,
        reason: document.getElementById('returnReason').value || "Customer request",
        items: items
    };

    try {
        const res = await fetch(`${API_BASE}/api/return-item`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast("Refund processed successfully", "success");
            document.getElementById('returnsModal').classList.add('hidden');
            await initApp(); // Refresh inventory
        } else {
            showToast("Failed to process return", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

// ─── TAB ROUTING SYSTEM ───────────────────────────────────────────────
function switchTab(tabId) {
    const searchBar = document.getElementById('topSearchBar');
    const btnOpenAdd = document.getElementById('btnOpenAddModal');

    if (searchBar) {
        searchBar.style.display = 'none';
    }
    if (btnOpenAdd) {
        btnOpenAdd.style.display = 'none';
    }

    document.querySelectorAll('.tab-content-panel').forEach(panel => {
        panel.classList.add('hidden');
        panel.classList.remove('active-panel');
    });

    document.querySelectorAll('.nav-tab').forEach(btn => {
        btn.classList.remove('active');
    });

    let panelId = 'tabContentInventory';
    let btnId = 'tabBtnInventory';

    if (tabId === 'inventory') { panelId = 'tabContentInventory'; btnId = 'tabBtnInventory'; loadInventoryTable(); }
    else if (tabId === 'recipes') { panelId = 'tabContentRecipes'; btnId = 'tabBtnRecipes'; loadRecipesTab(); }
    else if (tabId === 'import') { panelId = 'tabContentImport'; btnId = 'tabBtnImport'; }
    else if (tabId === 'reports') { panelId = 'tabContentReports'; btnId = 'tabBtnReports'; loadReportsData(); }

    const activePanel = document.getElementById(panelId);
    if (activePanel) {
        activePanel.classList.remove('hidden');
        activePanel.classList.add('active-panel');
    }

    const activeBtn = document.getElementById(btnId);
    if (activeBtn) {
        activeBtn.classList.add('active');
    }
}

// ─── INVENTORY MANAGEMENT ─────────────────────────────────────────────
function loadInventoryTable() {
    const grid = document.getElementById('inventoryGrid');
    if (!grid) return;
    grid.innerHTML = '';

    allProducts.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        
        // Priority: Item Image -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage && itemImage.length > 5 && itemImage.includes('/')) {
            displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
        } else if (catImage && catImage.length > 5 && catImage.includes('/')) {
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            displayContent = `<span class="emoji-icon">${itemImage || '📦'}</span>`;
        }

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})" title="Edit" style="position: absolute; top: 10px; right: 10px; width: 32px; height: 32px; background: rgba(255,255,255,0.05); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteIngredient(${p.id})" title="Delete" style="position: absolute; top: 10px; left: 10px; width: 32px; height: 32px; background: rgba(239, 68, 68, 0.05); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 15px;">${displayContent}</div>
            <div class="product-info" style="text-align: center;">
                <div class="product-name">${p.name}</div>
                <div class="product-price" style="color: var(--accent); font-weight: 800; font-size: 0.95rem; margin-top: 4px;">${formatPrice(p.price)}</div>
                <div class="product-stock ${p.stock < (p.minStock || 5) ? 'low' : ''}" style="font-size: 0.75rem; font-weight: 600;">Stock: ${p.stock}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openAddModalDirect() {
    document.getElementById('modalTitle').innerText = t('modal_add_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
    document.getElementById('editItemId').value = '';
    document.getElementById('newItemName').value = '';
    document.getElementById('newItemPrice').value = '';
    document.getElementById('newItemStock').value = '';
    document.getElementById('newItemBarcode').value = '';
    document.getElementById('addItemModal').classList.remove('hidden');
}

function showDeleteConfirm(title, message, onConfirm) {
    const overlay = document.createElement('div');
    overlay.className = 'overlay';
    overlay.style.zIndex = '99999';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.style.background = 'rgba(0, 0, 0, 0.4)';
    overlay.style.backdropFilter = 'blur(4px)';

    const card = document.createElement('div');
    card.className = 'scanner-card';
    card.style.maxWidth = '360px';
    card.style.padding = '24px';
    card.style.borderRadius = '16px';
    card.style.background = 'var(--bg-secondary)';
    card.style.border = '1px solid var(--border-color)';
    card.style.boxShadow = '0 10px 30px rgba(0,0,0,0.15)';
    card.style.textAlign = 'center';

    card.innerHTML = `
        <h3 style="margin-top:0; color:var(--text-main); font-size:1.2rem; font-weight:800;">${title}</h3>
        <p style="color:var(--text-muted); font-size:0.9rem; margin:16px 0 24px 0; line-height:1.5;">${message}</p>
        <div style="display:flex; gap:12px; justify-content:center;">
            <button class="btn-clear" id="confirmCancelBtn" style="flex:1; height:40px; border:1px solid var(--border-color); color:var(--text-main); border-radius:8px; font-weight:600; cursor:pointer;">Cancel</button>
            <button class="btn-primary" id="confirmOkBtn" style="flex:1; height:40px; background:var(--danger) !important; color:white; border:none; border-radius:8px; font-weight:600; cursor:pointer;">Delete</button>
        </div>
    `;

    overlay.appendChild(card);
    document.body.appendChild(overlay);

    const closeConfirm = () => {
        document.body.removeChild(overlay);
    };

    overlay.querySelector('#confirmCancelBtn').onclick = closeConfirm;
    overlay.querySelector('#confirmOkBtn').onclick = () => {
        closeConfirm();
        onConfirm();
    };
}

function deleteIngredient(id) {
    showDeleteConfirm(
        "Delete Ingredient",
        "Are you sure you want to delete this ingredient?",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/products/${id}`, { method: 'DELETE' });
                if (res.ok) {
                    showToast("Ingredient deleted successfully", "success");
                    await initApp();
                    loadInventoryTable();
                } else {
                    showToast("Failed to delete ingredient", "error");
                }
            } catch (e) { showToast("Connection error", "error"); }
        }
    );
}

// ─── RECIPES MANAGEMENT ───────────────────────────────────────────────
async function fetchRecipes() {
    try {
        const res = await fetch(`${API_BASE}/api/recipes`);
        if (res.ok) {
            allRecipes = await res.json();
        }
    } catch (e) { console.error("Fetch recipes failed", e); }
}

function loadRecipesTab() {
    const grid = document.getElementById('recipesGrid');
    if (!grid) return;
    grid.innerHTML = '';

    allRecipes.forEach(r => {
        const card = document.createElement('div');
        card.className = 'product-card';

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); editRecipe(${r.id})" title="Edit" style="position: absolute; top: 10px; right: 10px; width: 32px; height: 32px; background: rgba(255,255,255,0.05); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteRecipe(${r.id})" title="Delete" style="position: absolute; top: 10px; left: 10px; width: 32px; height: 32px; background: rgba(239, 68, 68, 0.05); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 15px;">
                <span class="emoji-icon">🍲</span>
            </div>
            <div class="product-info" style="text-align: center;">
                <div class="product-name">${r.name}</div>
                <div class="product-price" style="color: var(--accent); font-weight: 800; font-size: 0.95rem; margin-top: 4px;">${formatPrice(r.price)}</div>
                <div class="product-stock" style="font-size: 0.75rem; font-weight: 600; color: var(--text-muted);">Recipe</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openNewRecipeModal() {
    document.getElementById('recipeModalTitle').innerText = "Create New Recipe";
    document.getElementById('editRecipeId').value = '';
    document.getElementById('recipeName').value = '';
    document.getElementById('recipeDesc').value = '';
    document.getElementById('recipePrice').value = '';
    document.getElementById('recipeIngredientsList').innerHTML = '';
    addRecipeIngredientRow();
    document.getElementById('recipeModal').classList.remove('hidden');
}

function closeRecipeModal() {
    document.getElementById('recipeModal').classList.add('hidden');
}

function addRecipeIngredientRow(selectedPartId = '', qty = 1) {
    const list = document.getElementById('recipeIngredientsList');
    if (!list) return;

    const row = document.createElement('div');
    row.className = 'recipe-ingredient-row';
    row.style.display = 'grid';
    row.style.gridTemplateColumns = '1fr 80px 40px';
    row.style.gap = '10px';
    row.style.alignItems = 'center';

    let options = allProducts.map(p => `<option value="${p.id}" ${p.id == selectedPartId ? 'selected' : ''}>${p.name}</option>`).join('');

    row.innerHTML = `
        <select class="ingredient-select" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:1px solid var(--border); padding:8px; border-radius:6px;">
            <option value="">-- Choose Ingredient --</option>
            ${options}
        </select>
        <input type="number" class="ingredient-qty" placeholder="Qty" value="${qty}" min="0.1" step="0.1" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:1px solid var(--border); padding:8px; border-radius:6px; text-align:center;">
        <button onclick="removeRecipeIngredientRow(this)" style="background:none; border:none; color:var(--danger); font-size:1.3rem; cursor:pointer;">&times;</button>
    `;
    list.appendChild(row);
}

function removeRecipeIngredientRow(btn) {
    const row = btn.parentElement;
    row.remove();
}

async function saveRecipe() {
    const id = document.getElementById('editRecipeId').value;
    const name = document.getElementById('recipeName').value;
    const desc = document.getElementById('recipeDesc').value;
    const price = parseFloat(document.getElementById('recipePrice').value);

    if (!name || isNaN(price)) {
        showToast("Please fill Recipe Name and Selling Price", "error");
        return;
    }

    const ingredientRows = document.querySelectorAll('.recipe-ingredient-row');
    const ingredients = [];
    ingredientRows.forEach(row => {
        const partId = row.querySelector('.ingredient-select').value;
        const qty = parseFloat(row.querySelector('.ingredient-qty').value);
        if (partId && qty > 0) {
            ingredients.push({ partId: parseInt(partId), qty });
        }
    });

    if (ingredients.length === 0) {
        showToast("Add at least one valid ingredient", "error");
        return;
    }

    const payload = { id: id ? parseInt(id) : null, name, description: desc, price, ingredients };

    try {
        const res = await fetch(`${API_BASE}/api/recipes`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast(id ? "Recipe updated!" : "Recipe created!", "success");
            closeRecipeModal();
            await fetchRecipes();
            loadRecipesTab();
        } else {
            showToast("Failed to save recipe", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

function deleteRecipe(id) {
    showDeleteConfirm(
        "Delete Recipe",
        "Are you sure you want to delete this recipe? Components will remain in inventory.",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/recipes/${id}`, { method: 'DELETE' });
                if (res.ok) {
                    showToast("Recipe deleted successfully", "success");
                    await fetchRecipes();
                    loadRecipesTab();
                } else {
                    showToast("Failed to delete recipe", "error");
                }
            } catch (e) { showToast("Connection error", "error"); }
        }
    );
}

function editRecipe(id) {
    const r = allRecipes.find(x => x.id === id);
    if (!r) return;

    document.getElementById('recipeModalTitle').innerText = "Edit Recipe";
    document.getElementById('editRecipeId').value = r.id;
    document.getElementById('recipeName').value = r.name;
    document.getElementById('recipeDesc').value = r.description || '';
    document.getElementById('recipePrice').value = r.price;

    const list = document.getElementById('recipeIngredientsList');
    list.innerHTML = '';
    if (r.parts && r.parts.length > 0) {
        r.parts.forEach(p => {
            addRecipeIngredientRow(p.partId, p.qty);
        });
    } else {
        addRecipeIngredientRow();
    }
    document.getElementById('recipeModal').classList.remove('hidden');
}

// ─── CSV FILE IMPORT / EXPORT ────────────────────────────────────────
let pendingImportItems = [];

function handleImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function(evt) {
        const content = evt.target.result;
        parseImportFile(content, file.name.endsWith('.tsv') || file.name.endsWith('.xls') || content.includes('\t'));
    };
    reader.readAsText(file);
}

function parseImportFile(text, isTsv) {
    const lines = text.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    const separator = isTsv ? '\t' : ',';
    const headers = lines[0].split(separator).map(h => h.replace(/"/g, '').trim());
    
    // Auto-detect schema
    const nameIdx = headers.findIndex(h => h.toLowerCase().includes('name'));
    const catIdx = headers.findIndex(h => h.toLowerCase().includes('category'));
    const priceIdx = headers.findIndex(h => h.toLowerCase().includes('price') || h.toLowerCase().includes('unitprice'));
    const stockIdx = headers.findIndex(h => h.toLowerCase().includes('stock') || h.toLowerCase().includes('qty') || h.toLowerCase().includes('quantity'));
    const barcodeIdx = headers.findIndex(h => h.toLowerCase().includes('barcode'));
    const skuIdx = headers.findIndex(h => h.toLowerCase().includes('sku') || h.toLowerCase().includes('partnumber'));
    const descIdx = headers.findIndex(h => h.toLowerCase().includes('desc'));

    if (nameIdx === -1) {
        showToast("Invalid file format. 'Name' column is required.", "error");
        return;
    }

    pendingImportItems = [];
    for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
        if (cols.length < headers.length) continue;

        pendingImportItems.push({
            name: cols[nameIdx] || '',
            category: catIdx !== -1 ? cols[catIdx] : 'General',
            price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.00 : 0.00,
            stock: stockIdx !== -1 ? parseInt(cols[stockIdx]) || 0 : 0,
            barcode: barcodeIdx !== -1 ? cols[barcodeIdx] : '',
            sku: skuIdx !== -1 ? cols[skuIdx] : '',
            description: descIdx !== -1 ? cols[descIdx] : ''
        });
    }

    // Show preview
    document.getElementById('importPreviewCount').innerText = pendingImportItems.length;
    const previewList = document.getElementById('importPreviewList');
    previewList.innerHTML = pendingImportItems.map(item => `
        <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr 1fr; gap:10px;">
            <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${item.name}</b>
            <span style="color:var(--text-muted);">${item.category}</span>
            <span style="color:var(--accent); text-align:right;">$${item.price.toFixed(2)}</span>
            <span style="color:var(--text-main); text-align:right;">Qty: ${item.stock}</span>
        </div>
    `).join('');

    document.getElementById('importPreviewArea').classList.remove('hidden');
    showToast(`Parsed ${pendingImportItems.length} items from file`, "info");
}

function clearImportPreview() {
    pendingImportItems = [];
    document.getElementById('importFile').value = '';
    document.getElementById('importPreviewArea').classList.add('hidden');
    document.getElementById('importPreviewList').innerHTML = '';
}

async function confirmImport() {
    if (pendingImportItems.length === 0) return;
    try {
        const res = await fetch(`${API_BASE}/api/import-items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: pendingImportItems })
        });
        if (res.ok) {
            const result = await res.json();
            showToast(`Import Success! Imported: ${result.imported}, Skipped: ${result.skipped}`, "success");
            clearImportPreview();
            await initApp();
        } else {
            showToast("Failed to process import on server", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

async function exportInventoryToCsv() {
    try {
        // Construct CSV Content
        const headers = ["part_name", "category_name", "selling_price", "purchase_price", "quantity_in_stock", "minimum_stock_level", "barcode", "part_number", "description"];
        const rows = allProducts.map(p => [
            `"${p.name.replace(/"/g, '""')}"`,
            `"${(p.category || 'General').replace(/"/g, '""')}"`,
            p.price.toFixed(2),
            (p.price * 0.7).toFixed(2), // mock purchase price
            p.stock,
            p.minStock || 5,
            `"${(p.barcode || '').replace(/"/g, '""')}"`,
            `"${(p.sku || '').replace(/"/g, '""')}"`,
            `"${(p.description || '').replace(/"/g, '""')}"`
        ]);

        const csvContent = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const filename = `inventory_${new Date().toISOString().slice(0,10)}.csv`;

        const res = await fetch(`${API_BASE}/api/export-csv`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filename, csvContent })
        });

        if (res.ok) {
            const data = await res.json();
            showToast(`Exported CSV successfully to ${data.path}!`, "success");
        } else {
            showToast("Export failed on device", "error");
        }
    } catch (e) { showToast("Connection error during export", "error"); }
}

function downloadCsvTemplate() {
    const csvContent = "part_name,category_name,selling_price,quantity_in_stock,barcode,part_number,description\nSalmon Fillet,Seafood,15.99,100,72901234567,SF-100,Fresh pink salmon fillet\nTiger Prawns,Seafood,24.50,50,72909876543,TP-200,Frozen tiger prawns large\n";
    
    // In WebView, trigger download by writing to Downloads folder
    fetch(`${API_BASE}/api/export-csv`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filename: "Import_Template.csv", csvContent })
    })
    .then(res => {
        if (res.ok) showToast("Template downloaded to Downloads folder", "success");
        else showToast("Failed to download template", "error");
    })
    .catch(() => showToast("Connection error", "error"));
}

// ─── DAILY SALES IMPORT / EXPORT ─────────────────────────────────────
let pendingSalesItems = [];
let lastSalesDeductionResults = [];

function handleSalesImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function(evt) {
        const content = evt.target.result;
        parseSalesImportFile(content, file.name.endsWith('.tsv') || file.name.endsWith('.xls') || content.includes('\t'));
    };
    reader.readAsText(file);
}

function parseSalesImportFile(text, isTsv) {
    const lines = text.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    const separator = isTsv ? '\t' : ',';
    const headers = lines[0].split(separator).map(h => h.replace(/"/g, '').trim());
    
    // Detect recipe name and quantity sold columns
    const recipeIdx = headers.findIndex(h => h.toLowerCase().includes('recipe') || h.toLowerCase().includes('meal') || h.toLowerCase().includes('name'));
    const qtyIdx = headers.findIndex(h => h.toLowerCase().includes('qty') || h.toLowerCase().includes('quantity') || h.toLowerCase().includes('sold') || h.toLowerCase().includes('count'));

    if (recipeIdx === -1) {
        showToast("Invalid file format. 'Recipe Name' column is required.", "error");
        return;
    }
    const finalQtyIdx = qtyIdx !== -1 ? qtyIdx : -1;

    pendingSalesItems = [];
    for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
        if (cols.length < headers.length) continue;

        const recipeName = cols[recipeIdx] || '';
        const qtySold = finalQtyIdx !== -1 ? parseInt(cols[finalQtyIdx]) || 1 : 1;

        if (recipeName) {
            pendingSalesItems.push({
                recipeName,
                qtySold
            });
        }
    }

    // Hide results area when a new file is uploaded
    document.getElementById('salesImportResultsArea').classList.add('hidden');

    // Show preview
    document.getElementById('salesImportPreviewCount').innerText = pendingSalesItems.length;
    const previewList = document.getElementById('salesImportPreviewList');
    previewList.innerHTML = pendingSalesItems.map(item => `
        <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:flex; justify-content:space-between; align-items:center;">
            <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap; max-width:70%;">${item.recipeName}</b>
            <span style="color:var(--text-main); font-weight:700;">Qty: ${item.qtySold}</span>
        </div>
    `).join('');

    document.getElementById('salesImportPreviewArea').classList.remove('hidden');
    showToast(`Parsed ${pendingSalesItems.length} recipe sales from file`, "info");
}

function clearSalesImportPreview() {
    pendingSalesItems = [];
    document.getElementById('salesImportFile').value = '';
    document.getElementById('salesImportPreviewArea').classList.add('hidden');
    document.getElementById('salesImportPreviewList').innerHTML = '';
}

async function confirmSalesImport() {
    if (pendingSalesItems.length === 0) return;
    try {
        const res = await fetch(`${API_BASE}/api/import-sales`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sales: pendingSalesItems })
        });
        if (res.ok) {
            const result = await res.json();
            showToast(`Deduction complete! Processed: ${result.processed}, Skipped: ${result.skipped}`, "success");
            
            // Store deductions for CSV export
            lastSalesDeductionResults = result.deductions || [];

            // Display Results
            document.getElementById('salesResultProcessed').innerText = result.processed;
            document.getElementById('salesResultSkipped').innerText = result.skipped;

            const skippedContainer = document.getElementById('salesResultSkippedListContainer');
            if (result.skipped > 0 && result.skippedNames && result.skippedNames.length > 0) {
                document.getElementById('salesResultSkippedList').innerText = result.skippedNames.join(', ');
                skippedContainer.classList.remove('hidden');
            } else {
                skippedContainer.classList.add('hidden');
            }

            const resultsList = document.getElementById('salesImportResultsList');
            if (lastSalesDeductionResults.length > 0) {
                resultsList.innerHTML = lastSalesDeductionResults.map(d => {
                    const partName = d.partName || d.PartName || "";
                    const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
                    const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;
                    return `
                        <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:6px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr; gap:10px;">
                            <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${partName}</b>
                            <span style="color:var(--danger); text-align:right;">-${qtyDeducted}</span>
                            <span style="color:${newStock <= 0 ? 'var(--danger)' : newStock < 5 ? 'var(--warn)' : 'var(--text-main)'}; text-align:right; font-weight:700;">Stock: ${newStock}</span>
                        </div>
                    `;
                }).join('');
            } else {
                resultsList.innerHTML = '<div style="color:var(--text-muted); text-align:center; padding:10px;">No ingredient deductions occurred.</div>';
            }

            document.getElementById('salesImportResultsArea').classList.remove('hidden');
            clearSalesImportPreview();

            // Refresh the application inventory cache & reload tables
            await initApp();
            loadInventoryTable();
        } else {
            showToast("Failed to process sales deduction on server", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

async function downloadSalesConsumptionReport() {
    if (lastSalesDeductionResults.length === 0) {
        showToast("No consumption results to export", "warn");
        return;
    }
    try {
        const headers = ["Ingredient Name", "Quantity Deducted", "Previous Stock", "New Stock"];
        const rows = lastSalesDeductionResults.map(d => {
            const partName = d.partName || d.PartName || "";
            const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
            const previousStock = d.previousStock !== undefined ? d.previousStock : d.PreviousStock;
            const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;
            return [
                `"${partName.replace(/"/g, '""')}"`,
                qtyDeducted,
                previousStock,
                newStock
            ];
        });

        const csvContent = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const filename = `sales_consumption_${new Date().toISOString().slice(0,10)}.csv`;

        const res = await fetch(`${API_BASE}/api/export-csv`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filename, csvContent })
        });

        if (res.ok) {
            const data = await res.json();
            showToast(`Consumption report saved successfully to ${data.path}!`, "success");
        } else {
            showToast("Failed to export report on device", "error");
        }
    } catch (e) { showToast("Connection error during consumption export", "error"); }
}

// ─── REPORTS & ANALYTICS ──────────────────────────────────────────────
async function loadReportsData() {
    try {
        const res = await fetch(`${API_BASE}/api/reports`);
        if (res.ok) {
            const data = await res.json();
            
            // Populate KPIs
            document.getElementById('kpiRevenue').innerText = formatPrice(data.revenue || 0);
            document.getElementById('kpiOrders').innerText = data.orders || 0;
            document.getElementById('kpiOutOfStock').innerText = data.outOfStock || 0;
            document.getElementById('kpiLowStock').innerText = data.lowStock || 0;

            // Render category sales list
            const catList = document.getElementById('categorySalesList');
            if (catList) {
                catList.innerHTML = '';
                if (data.categorySales && data.categorySales.length > 0) {
                    const maxSales = Math.max(...data.categorySales.map(c => c.sales)) || 1;
                    
                    data.categorySales.forEach(c => {
                        const pct = (c.sales / maxSales) * 100;
                        const row = document.createElement('div');
                        row.className = 'report-bar-row';
                        row.innerHTML = `
                            <div class="report-bar-label">
                                <span style="font-weight:600; color:var(--text-main);">${c.category}</span>
                                <span style="color:var(--accent); font-weight:700;">${formatPrice(c.sales)}</span>
                            </div>
                            <div class="report-bar-outer">
                                <div class="report-bar-inner" style="width: ${pct}%;"></div>
                            </div>
                        `;
                        catList.appendChild(row);
                    });
                } else {
                    catList.innerHTML = '<div style="color:var(--text-muted); font-size:0.9rem; text-align:center; padding:20px;">No sales data available.</div>';
                }
            }

            // Render activity logs
            const logList = document.getElementById('activityLogsList');
            if (logList) {
                logList.innerHTML = '';
                if (data.transactions && data.transactions.length > 0) {
                    data.transactions.forEach(t => {
                        const div = document.createElement('div');
                        div.className = `activity-log-item ${t.action.toLowerCase()}`;
                        div.innerHTML = `
                            <div style="display:flex; justify-content:space-between; font-weight:700; color:var(--text-main);">
                                <span>${t.action}: ${t.item}</span>
                                <span style="font-size:0.75rem; color:var(--text-muted);">${t.user}</span>
                            </div>
                            <div style="color:var(--text-muted); margin-top:4px;">${t.desc}</div>
                            <div class="activity-time">${t.time}</div>
                        `;
                        logList.appendChild(div);
                    });
                } else {
                    logList.innerHTML = '<div style="color:var(--text-muted); font-size:0.9rem; text-align:center; padding:20px;">No recent activities found.</div>';
                }
            }
        }
    } catch (e) { console.error("Failed to load reports", e); }
}

window.clearReportsData = async function() {
    if (!confirm("Are you sure you want to clear all sales orders and activity logs to start a fresh daily report? This action cannot be undone.")) {
        return;
    }
    try {
        const res = await fetch(`${API_BASE}/api/clear-reports`, { method: 'POST' });
        if (res.ok) {
            showToast("Daily report cleared successfully!", "success");
            await loadReportsData();
        } else {
            showToast("Failed to clear reports", "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};


window.toggleCartDrawer = function() {
    const sidebar = document.querySelector('.cart-sidebar');
    const closeBtn = document.querySelector('.cart-close-btn');
    if (sidebar) {
        const isOpen = sidebar.classList.toggle('open');
        if (closeBtn) {
            closeBtn.style.display = isOpen ? 'block' : 'none';
        }
    }
};
