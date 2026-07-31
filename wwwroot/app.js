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
        modal_field_icon: 'Icon / Image',
        modal_choose_icon_btn: 'Choose Icon / Upload Image',
        icon_picker_title: 'Choose Icon / Image',
        icon_picker_device_gallery: 'Choose from Device Gallery',
        icon_picker_or_select_icon: 'Or select a preset icon',
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
        modal_field_icon: 'أيقونة / صورة',
        modal_choose_icon_btn: 'اختر أيقونة / تحميل صورة',
        icon_picker_title: 'اختر أيقونة / صورة',
        icon_picker_device_gallery: 'اختر من معرض الجهاز',
        icon_picker_or_select_icon: 'أو اختر أيقونة جاهزة',
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
                { category: 'Groceries', sales: 300.00 },
                { category: 'Vegetables', sales: 200.50 }
            ],
            transactions: [
                { action: 'SALE', item: 'POS Sale', desc: 'Order #101 -- Total: $24.99', user: 'Admin', time: '2026-07-02 14:15' },
                { action: 'STOCK_ADD', item: 'Fresh Salmon Fillet', desc: 'Added via Import (Qty: 50)', user: 'Admin', time: '2026-07-02 13:00' }
            ],
            cogs: 550.00,
            grossProfit: 700.50,
            profitMargin: 56.02,
            recipeSummary: [
                { name: 'Garlic Butter Salmon', qtySold: 20, revenue: 499.80, cost: 225.00, profit: 274.80 }
            ],
            ingredientConsumption: [
                { name: 'Fresh Salmon Fillet', openingStock: 80.00, purchased: 50.00, usedInRecipes: 20.00, soldDirectly: 10.00, closingStock: 100.00, unit: 'kg' },
                { name: 'Garlic Butter', openingStock: 20.00, purchased: 10.00, usedInRecipes: 10.00, soldDirectly: 0.00, closingStock: 20.00, unit: 'kg' }
            ],
            stockMovement: [
                { date: '2026-07-02 13:00:00', ingredient: 'Fresh Salmon Fillet', type: 'Purchase', quantity: 50.00, unit: 'kg', remainingStock: 130.00 },
                { date: '2026-07-02 14:15:00', ingredient: 'Fresh Salmon Fillet', type: 'Recipe Consumption', quantity: 20.00, unit: 'kg', remainingStock: 110.00 },
                { date: '2026-07-02 14:15:00', ingredient: 'Garlic Butter', type: 'Recipe Consumption', quantity: 10.00, unit: 'kg', remainingStock: 20.00 }
            ],
            financialSummary: {
                revenue: 1250.50,
                purchaseCost: 150.00,
                ingredientCost: 550.00,
                grossProfit: 700.50,
                profitMargin: 56.02,
                inventoryValue: 1200.00
            },
            chartsData: {
                salesByDay: { '2026-07-01': 200, '2026-07-02': 350, '2026-07-03': 150, '2026-07-04': 400, '2026-07-05': 150.50 },
                bestRecipes: [ { name: 'Garlic Butter Salmon', qty: 20 } ],
                mostIngredients: [ { name: 'Fresh Salmon Fillet', qty: 30 } ],
                revenueTrend: [
                    { month: 'Feb 2026', revenue: 800 },
                    { month: 'Mar 2026', revenue: 950 },
                    { month: 'Apr 2026', revenue: 1100 },
                    { month: 'May 2026', revenue: 1050 },
                    { month: 'Jun 2026', revenue: 1200 },
                    { month: 'Jul 2026', revenue: 1250.50 }
                ]
            }
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
            let imported = 0;
            let skipped = 0;

            body.items.forEach((item) => {
                if (!item.name) {
                    skipped++;
                    return;
                }

                // Check if existing
                let existing = mockDb.products.find(p => 
                    (item.barcode && p.barcode === item.barcode) ||
                    (item.sku && p.sku === item.sku) ||
                    (p.name.toLowerCase() === item.name.toLowerCase())
                );

                const pSize = parseFloat(item.packSize) || 1.0;
                const conv = parseFloat(item.conversionValue) || 1.0;
                const pPrice = parseFloat(item.packPrice || item.price || 0.00);

                const stockPacks = item.stock / pSize; // Current Stock from CSV is in Big Unit, converted to packs in database
                const costPerBigUnit = pPrice / pSize;
                const costPerSmallUnit = costPerBigUnit / conv;

                if (existing) {
                    existing.name = item.name;
                    existing.sku = item.sku || existing.sku;
                    existing.barcode = item.barcode || existing.barcode;
                    existing.itemNo = item.itemNo || existing.itemNo;
                    existing.category = item.category || existing.category;
                    existing.description = item.description || existing.description;
                    existing.bigUnit = item.bigUnit || existing.bigUnit;
                    existing.smallUnit = item.smallUnit || existing.smallUnit;
                    existing.conversionValue = conv;
                    existing.packSize = pSize;
                    existing.packPrice = pPrice;
                    existing.purchasePrice = pPrice;
                    existing.price = item.price || existing.price;
                    existing.stock = stockPacks;
                    existing.minStock = item.minStock || existing.minStock;
                    existing.itemPrice = costPerBigUnit;
                    existing.piecePrice = costPerSmallUnit;
                } else {
                    mockDb.products.push({
                        id: mockDb.products.length + 1,
                        name: item.name,
                        sku: item.sku || '',
                        barcode: item.barcode || '',
                        itemNo: item.itemNo || '',
                        category: item.category || 'General',
                        description: item.description || '',
                        bigUnit: item.bigUnit || '',
                        smallUnit: item.smallUnit || '',
                        conversionValue: conv,
                        packSize: pSize,
                        packPrice: pPrice,
                        purchasePrice: pPrice,
                        price: item.price || pPrice,
                        stock: stockPacks,
                        minStock: item.minStock || 5,
                        itemPrice: costPerBigUnit,
                        piecePrice: costPerSmallUnit
                    });
                }
                imported++;
            });
            responseData = { success: true, imported, skipped };
        } else if (path === 'api/import-sales') {
            const body = JSON.parse(options.body);
            let processed = 0;
            let skipped = 0;
            const skippedNames = [];
            const deductions = [];

            body.sales.forEach(sale => {
                const cleanName = sale.recipeName.toLowerCase().replace(/[\s\t\r\n]/g, '');
                
                // 1. Try finding recipe
                const recipe = mockDb.recipes.find(r => r.name.toLowerCase().replace(/[\s\t\r\n]/g, '') === cleanName);
                if (recipe) {
                    recipe.parts.forEach(part => {
                        const prod = mockDb.products.find(p => p.id === part.partId);
                        if (prod) {
                            const pSize = parseFloat(prod.packSize) || 1.0;
                            const conv = parseFloat(prod.conversionValue) || 1.0;
                            const partUom = (part.unitOfMeasure || '').toLowerCase().trim();
                            const sUnit = (prod.smallUnit || '').toLowerCase().trim();
                            const bUnit = (prod.bigUnit || '').toLowerCase().trim();

                            let qtyPerRecipeConverted = part.qty;
                            if (bUnit && sUnit) {
                                if (partUom === sUnit) {
                                    qtyPerRecipeConverted = part.qty / (pSize * conv);
                                } else if (partUom === bUnit) {
                                    qtyPerRecipeConverted = part.qty / pSize;
                                } else if (partUom === 'pack') {
                                    qtyPerRecipeConverted = part.qty;
                                } else {
                                    qtyPerRecipeConverted = part.qty / (pSize * conv);
                                }
                            }

                            const qtyDeducted = qtyPerRecipeConverted * sale.qtySold;
                            if (qtyDeducted <= 0) return;
                            
                            const prevStock = prod.stock;
                            prod.stock = Math.max(0, parseFloat((prod.stock - qtyDeducted).toFixed(4)));

                            const existingDeduct = deductions.find(d => d.partId === part.partId);
                            if (existingDeduct) {
                                existingDeduct.qtyDeducted = parseFloat((existingDeduct.qtyDeducted + qtyDeducted).toFixed(4));
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
                                desc: `Deducted ${qtyDeducted.toFixed(4).replace(/\.0000$/, '')} via sales import (${recipe.name} x${sale.qtySold})`,
                                user: 'Admin',
                                time: new Date().toISOString().replace('T', ' ').slice(0, 16)
                            });
                        }
                    });
                    processed++;
                } else {
                    // 2. Try finding product directly (Ingredient Sale)
                    const prod = mockDb.products.find(p => p.name.toLowerCase().replace(/[\s\t\r\n]/g, '') === cleanName);
                    if (prod) {
                        const pSize = parseFloat(prod.packSize) || 1.0;
                        const conv = parseFloat(prod.conversionValue) || 1.0;
                        const saleUom = (sale.unitOfMeasure || '').toLowerCase().trim();
                        const sUnit = (prod.smallUnit || '').toLowerCase().trim();
                        const bUnit = (prod.bigUnit || '').toLowerCase().trim();

                        let qtyConverted = sale.qtySold;
                        if (bUnit && sUnit) {
                            if (saleUom === sUnit) {
                                qtyConverted = sale.qtySold / (pSize * conv);
                            } else if (saleUom === bUnit) {
                                qtyConverted = sale.qtySold / pSize;
                            } else if (saleUom === 'pack') {
                                qtyConverted = sale.qtySold;
                            } else {
                                qtyConverted = sale.qtySold / (pSize * conv);
                            }
                        }

                        const qtyDeducted = qtyConverted;
                        if (qtyDeducted > 0) {
                            const prevStock = prod.stock;
                            prod.stock = Math.max(0, parseFloat((prod.stock - qtyDeducted).toFixed(4)));

                            const existingDeduct = deductions.find(d => d.partId === prod.id);
                            if (existingDeduct) {
                                existingDeduct.qtyDeducted = parseFloat((existingDeduct.qtyDeducted + qtyDeducted).toFixed(4));
                                existingDeduct.newStock = prod.stock;
                            } else {
                                deductions.push({
                                    partId: prod.id,
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
                                desc: `Deducted ${qtyDeducted.toFixed(4).replace(/\.0000$/, '')} via sales import (${prod.name} x${sale.qtySold})`,
                                user: 'Admin',
                                time: new Date().toISOString().replace('T', ' ').slice(0, 16)
                            });
                        }
                        processed++;
                    } else {
                        skipped++;
                        skippedNames.push(sale.recipeName);
                    }
                }
            });

            responseData = { success: true, processed, skipped, skippedNames, deductions };
        } else if (path === 'api/export-csv') {
            responseData = { success: true, path: 'Downloads/mock_export.csv' };
        } else if (path.startsWith('api/reports')) {
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
let selectedProductIcon = '📦';
let selectedRecipeIcon = '🍲';
let iconPickerTarget = 'product'; // 'product' or 'recipe'

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
        document.getElementById('newItemNo').value = '';
        document.getElementById('newItemName').value = '';
        document.getElementById('newItemCategory').value = '';
        document.getElementById('newItemStock').value = '0';
        document.getElementById('newItemMinStock').value = '5';
        document.getElementById('newItemBigUnit').value = '';
        document.getElementById('newItemSmallUnit').value = '';
        document.getElementById('newItemConversionValue').value = '1';
        document.getElementById('newItemPackSize').value = '';
        document.getElementById('newItemPackPrice').value = '';
        document.getElementById('newItemBarcode').value = '';
        
        selectedProductIcon = '📦';
        updateIconPreview('product', '📦');
        
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

    // Handle file input selection
    safeListen('iconFileInput', 'change', function(e) {
        const file = e.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function(evt) {
            const img = new Image();
            img.onload = function() {
                const canvas = document.createElement('canvas');
                const maxDimension = 200;
                let width = img.width;
                let height = img.height;
                
                if (width > height) {
                    if (width > maxDimension) {
                        height = Math.round((height * maxDimension) / width);
                        width = maxDimension;
                    }
                } else {
                    if (height > maxDimension) {
                        width = Math.round((width * maxDimension) / height);
                        height = maxDimension;
                    }
                }
                
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);
                
                const dataUrl = canvas.toDataURL('image/jpeg', 0.8);
                if (iconPickerTarget === 'product') {
                    selectedProductIcon = dataUrl;
                    updateIconPreview('product', dataUrl);
                } else {
                    selectedRecipeIcon = dataUrl;
                    updateIconPreview('recipe', dataUrl);
                }
                closeIconPickerModal();
                // Reset file input so same file can be chosen again if needed
                document.getElementById('iconFileInput').value = '';
            };
            img.src = evt.target.result;
        };
        reader.readAsDataURL(file);
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
    
    // Sync the Category datalist
    const datalist = document.getElementById('categoryDatalist');
    if (datalist) {
        datalist.innerHTML = '';
        masterCategories.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat;
            datalist.appendChild(opt);
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
            const subtext = r.categoryName ? `Recipe • ${r.categoryName}` : "Recipe";
            const recipeImage = r.image;
            let displayContent = `<span class="emoji-icon">🍲</span>`;
            if (recipeImage) {
                if (isImagePath(recipeImage)) {
                    displayContent = `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
                } else {
                    displayContent = `<span class="emoji-icon">${recipeImage}</span>`;
                }
            }
            card.innerHTML = `
                <div class="product-img">${displayContent}</div>
                <div class="product-info">
                    <div class="product-name">${r.name}</div>
                    <div class="product-price">${formatPrice(r.price)}</div>
                    <div class="product-stock" style="color:var(--accent);">${subtext}</div>
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
        
        // Priority: Item Image (Path/Data URL) -> Item Emoji -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage) {
            if (isImagePath(itemImage)) {
                displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${itemImage}</span>`;
            }
        } else if (catImage && isImagePath(catImage)) {
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            displayContent = `<span class="emoji-icon">📦</span>`;
        }

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${p.name}</div>
                <div class="product-price">${formatPrice(p.price)}</div>
                <div class="product-stock ${p.stock < (p.minStock || 5) ? 'low' : ''}">${t('stock_label')}: ${p.stock} ${p.unitOfMeasure || 'pcs'}</div>
            </div>
        `;
        grid.appendChild(card);
    });

    // Also display recipes that belong to this category!
    const filteredRecipes = allRecipes.filter(r => {
        const matchesCategory = currentCategory === 'All' || r.categoryName === currentCategory;
        const matchesSearch = !query || (r.name && r.name.toLowerCase().includes(query));
        return matchesCategory && matchesSearch;
    });

    filteredRecipes.forEach(r => {
        const card = document.createElement('div');
        card.className = 'product-card recipe-product-card';
        card.onclick = () => addRecipeToCart(r);
        const subtext = r.categoryName ? `Recipe • ${r.categoryName}` : "Recipe";
        const recipeImage = r.image;
        let displayContent = `<span class="emoji-icon">🍲</span>`;
        if (recipeImage) {
            if (isImagePath(recipeImage)) {
                displayContent = `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${recipeImage}</span>`;
            }
        }
        card.innerHTML = `
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${r.name}</div>
                <div class="product-price">${formatPrice(r.price)}</div>
                <div class="product-stock" style="color:var(--accent);">${subtext}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

window.openAddModalDirect = function() {
    document.getElementById('modalTitle').innerText = t('modal_add_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
    document.getElementById('editItemId').value = '';
    document.getElementById('newItemNo').value = '';
    document.getElementById('newItemName').value = '';
    document.getElementById('newItemCategory').value = '';
    document.getElementById('newItemStock').value = '0';
    document.getElementById('newItemMinStock').value = '5';
    document.getElementById('newItemBigUnit').value = '';
    document.getElementById('newItemSmallUnit').value = '';
    document.getElementById('newItemConversionValue').value = '1';
    document.getElementById('newItemPackSize').value = '';
    document.getElementById('newItemPackPrice').value = '';
    document.getElementById('newItemBarcode').value = '';
    
    selectedProductIcon = '📦';
    updateIconPreview('product', '📦');
    
    document.getElementById('addItemModal').classList.remove('hidden');
};

function openEditModal(id) {
    const item = allProducts.find(p => p.id === id);
    if (!item) return;
    document.getElementById('modalTitle').innerText = t('modal_edit_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_save_btn');
    document.getElementById('editItemId').value = item.id;
    document.getElementById('newItemNo').value = item.itemNo || '';
    document.getElementById('newItemName').value = item.name;
    document.getElementById('newItemCategory').value = item.category || (masterCategories.length > 0 ? masterCategories[0] : '');
    document.getElementById('newItemStock').value = item.stock;
    document.getElementById('newItemMinStock').value = item.minStock !== undefined ? item.minStock : 5;
    document.getElementById('newItemBarcode').value = item.barcode || '';

    document.getElementById('newItemBigUnit').value = item.bigUnit || '';
    document.getElementById('newItemSmallUnit').value = item.smallUnit || '';
    document.getElementById('newItemConversionValue').value = item.conversionValue || '1';
    document.getElementById('newItemPackSize').value = item.packSize || '';
    document.getElementById('newItemPackPrice').value = item.packPrice || item.price || '';

    selectedProductIcon = item.image || '📦';
    updateIconPreview('product', selectedProductIcon);

    document.getElementById('addItemModal').classList.remove('hidden');
}

async function submitNewItem() {
    const editId = document.getElementById('editItemId').value;
    const parsedId = editId ? parseInt(editId) : null;

    const packPrice = parseFloat(document.getElementById('newItemPackPrice').value) || 0;
    const minStock = parseInt(document.getElementById('newItemMinStock').value) || 5;

    const bigUnit = document.getElementById('newItemBigUnit').value.trim();
    const smallUnit = document.getElementById('newItemSmallUnit').value.trim();
    const conversionValue = parseFloat(document.getElementById('newItemConversionValue').value) || 1.0;
    const packSize = parseFloat(document.getElementById('newItemPackSize').value) || 1.0;
    const itemPrice = packSize > 0 ? (packPrice / packSize) : 0;

    const itemData = {
        itemNo: document.getElementById('newItemNo').value,
        name: document.getElementById('newItemName').value,
        category: document.getElementById('newItemCategory').value,
        price: packPrice, // The main price is the pack price
        stock: parseInt(document.getElementById('newItemStock').value) || 0,
        barcode: document.getElementById('newItemBarcode').value,
        unitOfMeasure: bigUnit, // Use Big Unit as the base unit of measure
        stockType: 'Pack', // Always Pack item by default
        packItemsNumber: 1,
        packPrice: packPrice,
        itemPrice: itemPrice,
        piecePrice: 0,
        minStock: minStock,
        bigUnit: bigUnit,
        smallUnit: smallUnit,
        conversionValue: conversionValue,
        packSize: packSize,
        image: selectedProductIcon
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
        const defaultUom = (product.bigUnit && product.smallUnit) ? 'pack' : (product.unitOfMeasure || 'pcs');
        cart.push({ ...product, quantity: 1, itemType: 'Part', selectedUom: defaultUom, basePrice: product.price });
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

        let uomSelectHtml = '';
        if (item.bigUnit && item.smallUnit) {
            uomSelectHtml = `
                <select class="cart-item-uom" onchange="changeCartItemUom(${item.id}, this.value, '${item.itemType || 'Part'}')" style="padding:2px 4px; background:rgba(0,0,0,0.3); color:white; border:1px solid var(--border); border-radius:4px; font-size:0.75rem; margin-top:4px; outline:none; cursor:pointer; font-weight:bold;">
                    <option value="pack" ${item.selectedUom === 'pack' ? 'selected' : ''}>pack</option>
                    <option value="${item.bigUnit.toLowerCase()}" ${item.selectedUom === item.bigUnit.toLowerCase() ? 'selected' : ''}>${item.bigUnit}</option>
                    <option value="${item.smallUnit.toLowerCase()}" ${item.selectedUom === item.smallUnit.toLowerCase() ? 'selected' : ''}>${item.smallUnit}</option>
                </select>
            `;
        }

        div.innerHTML = `
            <div class="cart-item-info">
                <div class="cart-item-name">${item.name}</div>
                ${uomSelectHtml}
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

window.changeCartItemUom = function(id, val, itemType = 'Part') {
    const item = cart.find(x => x.id === id && x.itemType === itemType);
    if (!item) return;
    
    item.selectedUom = val;
    const basePrice = item.basePrice || item.price;
    const convVal = parseFloat(item.conversionValue) || 1.0;
    const packSz = parseFloat(item.packSize) || 1.0;
    
    if (val === 'pack') {
        item.price = basePrice;
    } else if (val === item.bigUnit.toLowerCase()) {
        item.price = basePrice / packSz;
    } else if (val === item.smallUnit.toLowerCase()) {
        item.price = (basePrice / packSz) / convVal;
    }
    updateCartUI();
};

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
            recipeId: i.itemType === 'Recipe' ? i.id : null,
            unitOfMeasure: i.selectedUom || 'pack'
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
    else if (tabId === 'stock') { panelId = 'tabContentStock'; btnId = 'tabBtnStock'; loadStockTab(); }
    else if (tabId === 'recipes') { panelId = 'tabContentRecipes'; btnId = 'tabBtnRecipes'; loadRecipesTab(); }
    else if (tabId === 'sales') { panelId = 'tabContentSales'; btnId = 'tabBtnSales'; loadSalesTab(); }
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
function populateInventoryCategoryFilter() {
    const filterSelect = document.getElementById('inventoryCategoryFilter');
    if (!filterSelect) return;

    const currentVal = filterSelect.value || 'All';

    // Collect unique categories
    const categories = new Set();
    allProducts.forEach(p => {
        if (p.category) categories.add(p.category);
    });

    if (typeof masterCategories !== 'undefined' && Array.isArray(masterCategories)) {
        masterCategories.forEach(c => {
            if (c) categories.add(c);
        });
    }

    // Sort categories
    const sortedCats = Array.from(categories).sort();

    // Rebuild options
    let html = '<option value="All">All Categories</option>';
    sortedCats.forEach(cat => {
        html += `<option value="${cat}">${cat}</option>`;
    });
    filterSelect.innerHTML = html;

    // Restore value
    if (currentVal === 'All' || sortedCats.includes(currentVal)) {
        filterSelect.value = currentVal;
    } else {
        filterSelect.value = 'All';
    }
}

window.filterInventoryByCategory = function() {
    loadInventoryTable();
};

function loadInventoryTable() {
    const grid = document.getElementById('inventoryGrid');
    if (!grid) return;

    populateInventoryCategoryFilter();

    const filterSelect = document.getElementById('inventoryCategoryFilter');
    const selectedCategory = filterSelect ? filterSelect.value : 'All';

    grid.innerHTML = '';

    const filteredProducts = selectedCategory === 'All' 
        ? allProducts 
        : allProducts.filter(p => p.category === selectedCategory);

    filteredProducts.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        
        // Priority: Item Image (Path/Data URL) -> Item Emoji -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage) {
            if (isImagePath(itemImage)) {
                displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${itemImage}</span>`;
            }
        } else if (catImage && isImagePath(catImage)) {
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            displayContent = `<span class="emoji-icon">📦</span>`;
        }

        let stockDetailsHtml = '';
        if (p.bigUnit && p.smallUnit) {
            stockDetailsHtml = `
                <div style="font-size: 0.72rem; color: var(--text-muted); margin-top: 4px; line-height: 1.2;">
                    Pack Price: <span style="color: var(--accent); font-weight: 600;">${formatPrice(p.packPrice || p.price)}</span><br/>
                    Pack Qty: <span style="color: var(--text); font-weight: 600;">${p.packSize} ${p.bigUnit}</span><br/>
                    Unit Price: <span style="color: var(--text); font-weight: 600;">${formatPrice(p.itemPrice)}/${p.bigUnit}</span>
                </div>
            `;
        } else {
            stockDetailsHtml = `
                <div style="font-size: 0.72rem; color: var(--text-muted); margin-top: 4px; line-height: 1.2;">
                    Price: <span style="color: var(--accent); font-weight: 600;">${formatPrice(p.price)}</span>
                </div>
            `;
        }

        const currentStockVal = p.bigUnit && p.packSize ? (p.stock * p.packSize).toFixed(2) : p.stock;
        const currentStockUom = p.bigUnit || 'pcs';
        const minStockVal = p.bigUnit && p.packSize ? (p.minStock * p.packSize).toFixed(2) : (p.minStock || 5);

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})" title="Edit" style="position: absolute; top: 6px; right: 6px; width: 24px; height: 24px; background: rgba(255,255,255,0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteIngredient(${p.id})" title="Delete" style="position: absolute; top: 6px; left: 6px; width: 24px; height: 24px; background: rgba(239, 68, 68, 0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 20px;">${displayContent}</div>
            <div class="product-info" style="text-align: center;">
                <div class="product-name" style="font-weight: 700;">${p.name}</div>
                ${stockDetailsHtml}
                <div class="product-stock ${p.stock < (p.minStock || 5) ? 'low' : ''}" style="font-size: 0.75rem; font-weight: 700; margin-top: 6px;">Stock: ${currentStockVal} ${currentStockUom} <span style="font-weight: 400; color: var(--text-muted);">(Min: ${minStockVal} ${currentStockUom})</span></div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openAddModalDirect() {
    window.openAddModalDirect();
}

function showDeleteConfirm(title, message, onConfirm, confirmText = 'Delete') {
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
            <button class="btn-primary" id="confirmOkBtn" style="flex:1; height:40px; background:var(--danger) !important; color:white; border:none; border-radius:8px; font-weight:600; cursor:pointer;">${confirmText}</button>
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
        const res = await fetch(`${API_BASE}/api/recipes?_=${Date.now()}`);
        if (res.ok) {
            const data = await res.json();
            if (data && data.error) {
                showToast(`Error loading recipes: ${data.error}`, "error");
                allRecipes = [];
            } else {
                allRecipes = Array.isArray(data) ? data : [];
            }
        } else {
            console.error("Fetch recipes failed with status", res.status);
            allRecipes = [];
        }
    } catch (e) { 
        console.error("Fetch recipes failed", e); 
        allRecipes = [];
    }
}

function loadRecipesTab() {
    const grid = document.getElementById('recipesGrid');
    if (!grid) return;
    grid.innerHTML = '';

    allRecipes.forEach(r => {
        const card = document.createElement('div');
        card.className = 'product-card';

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); editRecipe(${r.id})" title="Edit" style="position: absolute; top: 6px; right: 6px; width: 24px; height: 24px; background: rgba(255,255,255,0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteRecipe(${r.id})" title="Delete" style="position: absolute; top: 6px; left: 6px; width: 24px; height: 24px; background: rgba(239, 68, 68, 0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 20px;">
                ${(() => {
                    const recipeImage = r.image;
                    if (recipeImage) {
                        if (isImagePath(recipeImage)) {
                            return `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
                        } else {
                            return `<span class="emoji-icon">${recipeImage}</span>`;
                        }
                    }
                    return `<span class="emoji-icon">🍲</span>`;
                })()}
            </div>
            <div class="product-info" style="text-align: center; padding: 10px;">
                <div class="product-name">${r.name}</div>
                <div style="display:flex; justify-content:space-around; align-items:center; margin-top:8px;">
                    <div style="display:flex; flex-direction:column; align-items:center;">
                        <span style="font-size:0.7rem; color:var(--text-muted); font-weight:600; text-transform:uppercase;">Cost</span>
                        <span style="color:var(--text); font-weight:700; font-size:0.85rem;">${formatPrice(r.totalCost || 0)}</span>
                    </div>
                    <div style="display:flex; flex-direction:column; align-items:center;">
                        <span style="font-size:0.7rem; color:var(--text-muted); font-weight:600; text-transform:uppercase;">Price</span>
                        <span style="color:var(--accent); font-weight:800; font-size:0.9rem;">${formatPrice(r.price)}</span>
                    </div>
                </div>
                <div class="product-stock" style="font-size: 0.75rem; font-weight: 600; color: var(--text-muted); margin-top: 8px;">${r.categoryName ? `Recipe • ${r.categoryName}` : 'Recipe'}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openNewRecipeModal() {
    document.getElementById('recipeModalTitle').innerText = "Create New Recipe";
    document.getElementById('editRecipeId').value = '';
    document.getElementById('recipeItemNo').value = '';
    document.getElementById('recipeName').value = '';
    document.getElementById('recipeCategory').value = '';
    document.getElementById('recipeDesc').value = '';
    document.getElementById('recipePrice').value = '';
    document.getElementById('recipeIngredientsList').innerHTML = '';
    
    selectedRecipeIcon = '🍲';
    updateIconPreview('recipe', '🍲');
    
    addRecipeIngredientRow();
    document.getElementById('recipeModal').classList.remove('hidden');
}

function closeRecipeModal() {
    document.getElementById('recipeModal').classList.add('hidden');
}

function addRecipeIngredientRow(selectedPartId = '', qty = 1, savedUom = '') {
    const list = document.getElementById('recipeIngredientsList');
    if (!list) return;

    const row = document.createElement('div');
    row.className = 'recipe-ingredient-row';
    row.style.display = 'grid';
    row.style.gridTemplateColumns = '2fr 80px 100px 90px 90px 30px';
    row.style.gap = '10px';
    row.style.alignItems = 'center';
    row.style.marginBottom = '8px';

    let options = allProducts.map(p => `<option value="${p.id}" ${p.id == selectedPartId ? 'selected' : ''}>${p.name}</option>`).join('');

    let uomOptions = '';
    if (selectedPartId) {
        const product = allProducts.find(p => p.id == selectedPartId);
        if (product) {
            uomOptions = getUomOptionsHtml(product, savedUom);
        }
    }
    if (!uomOptions) {
        uomOptions = getUomOptionsHtml('pcs', savedUom);
    }

    row.innerHTML = `
        <select class="ingredient-select" onchange="onIngredientProductChange(this)" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px;">
            <option value="">-- Choose Ingredient --</option>
            ${options}
        </select>
        <input type="number" class="ingredient-qty" oninput="updateRecipeModalCosts()" placeholder="Qty" value="${qty}" min="0.01" step="0.01" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px; text-align:center;">
        <select class="ingredient-uom" onchange="updateRecipeModalCosts()" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px; font-weight:600; text-align:center; font-size:0.9rem;">
            ${uomOptions}
        </select>
        <span class="ingredient-unit-cost" style="color:var(--text-muted); text-align:right; font-size:0.85rem; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">$0.00</span>
        <span class="ingredient-total-cost" style="color:var(--accent); font-weight:bold; text-align:right; font-size:0.95rem;">$0.00</span>
        <button onclick="removeRecipeIngredientRow(this)" style="background:none; border:none; color:var(--danger); font-size:1.3rem; cursor:pointer; padding:0; line-height:1;">&times;</button>
    `;
    list.appendChild(row);
    updateRecipeModalCosts();
}

window.onIngredientProductChange = function(selectElem) {
    const row = selectElem.parentElement;
    const uomSelect = row.querySelector('.ingredient-uom');
    if (!uomSelect) return;

    const partId = parseInt(selectElem.value);
    if (partId) {
        const product = allProducts.find(p => p.id === partId);
        if (product) {
            uomSelect.innerHTML = getUomOptionsHtml(product);
        }
    } else {
        uomSelect.innerHTML = getUomOptionsHtml('pcs');
    }
    updateRecipeModalCosts();
};

function getUomOptionsHtml(prodOrUom, selectedUom = '') {
    let baseUom = 'pcs';
    let stockType = 'Piece';
    let bigUnit = '';
    let smallUnit = '';
    if (typeof prodOrUom === 'object' && prodOrUom !== null) {
        baseUom = prodOrUom.unitOfMeasure || 'pcs';
        stockType = prodOrUom.stockType || 'Piece';
        bigUnit = prodOrUom.bigUnit || '';
        smallUnit = prodOrUom.smallUnit || '';
    } else if (typeof prodOrUom === 'string') {
        baseUom = prodOrUom;
    }

    baseUom = baseUom.toLowerCase().trim();
    if (!selectedUom) selectedUom = baseUom;
    selectedUom = selectedUom.toLowerCase().trim();

    if (bigUnit && smallUnit) {
        const options = [
            { val: 'pack', text: 'pack' },
            { val: bigUnit.toLowerCase(), text: bigUnit },
            { val: smallUnit.toLowerCase(), text: smallUnit }
        ];
        return options.map(o => `<option value="${o.val}" ${o.val === selectedUom.toLowerCase() ? 'selected' : ''}>${o.text}</option>`).join('');
    }

    if (selectedUom.startsWith('gram') || selectedUom === 'g') selectedUom = 'g';
    else if (selectedUom.startsWith('kilo') || selectedUom === 'kg') selectedUom = 'kg';
    else if (selectedUom.startsWith('liter') || selectedUom === 'l') selectedUom = 'l';
    else if (selectedUom === 'ml') selectedUom = 'ml';
    else selectedUom = 'pcs';

    let options = [
        { val: 'pcs', text: 'pcs' },
        { val: 'g', text: 'gram' },
        { val: 'kg', text: 'kg' }
    ];

    if (baseUom.startsWith('l') || baseUom.startsWith('ml')) {
        options = [
            { val: 'ml', text: 'ml' },
            { val: 'l', text: 'l' },
            { val: 'pcs', text: 'pcs' }
        ];
    } else if (stockType === 'Pack') {
        options = [
            { val: 'pcs', text: 'pcs' },
            { val: 'g', text: 'gram' }
        ];
    }

    return options.map(o => `<option value="${o.val}" ${o.val === selectedUom ? 'selected' : ''}>${o.text}</option>`).join('');
}

function removeRecipeIngredientRow(btn) {
    const row = btn.parentElement;
    row.remove();
    updateRecipeModalCosts();
}

window.updateRecipeModalCosts = function() {
    let totalCost = 0;
    const rows = document.querySelectorAll('.recipe-ingredient-row');
    rows.forEach(row => {
        const select = row.querySelector('.ingredient-select');
        const qtyInput = row.querySelector('.ingredient-qty');
        const uomSelect = row.querySelector('.ingredient-uom');
        const unitCostSpan = row.querySelector('.ingredient-unit-cost');
        const totalCostSpan = row.querySelector('.ingredient-total-cost');
        
        if (!select || !qtyInput || !uomSelect) return;
        
        const partId = parseInt(select.value);
        const qty = parseFloat(qtyInput.value) || 0;
        const chosenUom = uomSelect.value.toLowerCase().trim();
        
        if (partId) {
            const product = allProducts.find(p => p.id === partId);
            if (product) {
                const baseUom = (product.unitOfMeasure || 'pcs').toLowerCase().trim();
                const baseCost = product.purchasePrice || 0;
                let convertedCost = baseCost;

                const bigUnit = (product.bigUnit || '').toLowerCase().trim();
                const smallUnit = (product.smallUnit || '').toLowerCase().trim();

                if (bigUnit && smallUnit) {
                    const convVal = parseFloat(product.conversionValue) || 1.0;
                    const packSz = parseFloat(product.packSize) || 1.0;

                    if (chosenUom === 'pack') {
                        convertedCost = baseCost;
                    } else if (chosenUom === bigUnit) {
                        convertedCost = baseCost / packSz;
                    } else if (chosenUom === smallUnit) {
                        convertedCost = (baseCost / packSz) / convVal;
                    } else {
                        convertedCost = baseCost;
                    }
                } else if (product.stockType === 'Pack') {
                    const packItems = product.packItemsNumber || 1;
                    const itemCost = baseCost / packItems;
                    if (chosenUom === 'pcs') {
                        convertedCost = itemCost;
                    } else if (chosenUom === 'g') {
                        if (baseUom.startsWith('kilo') || baseUom === 'kg') {
                            convertedCost = itemCost / 1000;
                        } else {
                            convertedCost = itemCost;
                        }
                    } else if (chosenUom === 'kg') {
                        if (baseUom.startsWith('gram') || baseUom === 'g') {
                            convertedCost = itemCost * 1000;
                        } else {
                            convertedCost = itemCost;
                        }
                    } else {
                        convertedCost = itemCost;
                    }
                } else {
                    if ((baseUom.startsWith('kilo') || baseUom === 'kg') && chosenUom === 'g') {
                        convertedCost = baseCost / 1000;
                    } else if ((baseUom.startsWith('gram') || baseUom === 'g') && chosenUom === 'kg') {
                        convertedCost = baseCost * 1000;
                    } else if ((baseUom.startsWith('liter') || baseUom === 'l') && chosenUom === 'ml') {
                        convertedCost = baseCost / 1000;
                    } else if (baseUom === 'ml' && chosenUom === 'l') {
                        convertedCost = baseCost * 1000;
                    }
                }

                const rowCost = convertedCost * qty;
                totalCost += rowCost;
                
                if (unitCostSpan) unitCostSpan.textContent = `$${convertedCost.toFixed(3)}/${chosenUom}`;
                if (totalCostSpan) totalCostSpan.textContent = `$${rowCost.toFixed(2)}`;
            }
        } else {
            if (unitCostSpan) unitCostSpan.textContent = '$0.00';
            if (totalCostSpan) totalCostSpan.textContent = '$0.00';
        }
    });
    
    const totalDisplay = document.getElementById('recipeIngredientsTotalCost');
    if (totalDisplay) {
        totalDisplay.textContent = `$${totalCost.toFixed(2)}`;
    }
};

async function saveRecipe() {
    const id = document.getElementById('editRecipeId').value;
    const itemNo = document.getElementById('recipeItemNo').value;
    const name = document.getElementById('recipeName').value;
    const categoryName = document.getElementById('recipeCategory').value;
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
        const uomSelect = row.querySelector('.ingredient-uom');
        const uom = uomSelect ? uomSelect.value : 'pcs';
        if (partId && qty > 0) {
            ingredients.push({ partId: parseInt(partId), qty, unitOfMeasure: uom });
        }
    });

    if (ingredients.length === 0) {
        showToast("Add at least one valid ingredient", "error");
        return;
    }

    const payload = { id: id ? parseInt(id) : null, name, itemNo, categoryName, description: desc, price, ingredients, image: selectedRecipeIcon };

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
    document.getElementById('recipeItemNo').value = r.itemNo || '';
    document.getElementById('recipeName').value = r.name;
    document.getElementById('recipeCategory').value = r.categoryName || '';
    document.getElementById('recipeDesc').value = r.description || '';
    document.getElementById('recipePrice').value = r.price;

    selectedRecipeIcon = r.image || '🍲';
    updateIconPreview('recipe', selectedRecipeIcon);

    const list = document.getElementById('recipeIngredientsList');
    list.innerHTML = '';
    if (r.parts && r.parts.length > 0) {
        r.parts.forEach(p => {
            addRecipeIngredientRow(p.partId, p.qty, p.unitOfMeasure);
        });
    } else {
        addRecipeIngredientRow();
    }
    document.getElementById('recipeModal').classList.remove('hidden');
}

let pendingImportItems = [];
let pendingImportType = 'ingredients'; // 'ingredients' or 'recipes'

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
    
    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const recipeNameIdx = findColIdx(['recipe name', 'recipe_name', 'recipe']);

    if (recipeNameIdx !== -1) {
        // --- RECIPE CSV IMPORT ---
        pendingImportType = 'recipes';
        
        const catIdx = findColIdx(['category', 'category name', 'categoryname', 'recipe category']);
        const priceIdx = findColIdx(['selling price', 'price', 'recipe price', 'sellingprice']);
        const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
        const itemNoIdx = findColIdx(['item no', 'itemno', 'no.', 'recipe item no']);
        const ingNameIdx = findColIdx(['ingredient name', 'ingredientname', 'ingredient', 'part name', 'partname', 'part']);
        const ingQtyIdx = findColIdx(['ingredient quantity', 'ingredientquantity', 'quantity', 'qty', 'ingredient qty', 'ingredientqty', 'ingredient_quantity']);
        const ingUomIdx = findColIdx(['measurement unit', 'measurementunit', 'unit of measure', 'unitofmeasure', 'uom', 'unit']);

        const recipesMap = {};
        for (let i = 1; i < lines.length; i++) {
            const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
            if (cols.length < headers.length) continue;

            const rName = cols[recipeNameIdx];
            if (!rName) continue;

            if (!recipesMap[rName]) {
                recipesMap[rName] = {
                    name: rName,
                    categoryName: catIdx !== -1 ? cols[catIdx] : 'General',
                    price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.0 : 0.0,
                    description: descIdx !== -1 ? cols[descIdx] : '',
                    itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                    ingredients: []
                };
            }

            const ingName = ingNameIdx !== -1 ? cols[ingNameIdx] : '';
            const ingQty = ingQtyIdx !== -1 ? parseFloat(cols[ingQtyIdx]) || 1.0 : 1.0;
            const ingUom = ingUomIdx !== -1 ? cols[ingUomIdx] : '';

            if (ingName) {
                recipesMap[rName].ingredients.push({
                    name: ingName,
                    qty: ingQty,
                    unitOfMeasure: ingUom
                });
            }
        }

        pendingImportItems = Object.values(recipesMap);

        if (pendingImportItems.length === 0) {
            showToast("No valid recipes found in file", "warn");
            return;
        }

        // Show recipe import preview
        document.getElementById('importPreviewCount').innerText = pendingImportItems.length + " recipes";
        const previewList = document.getElementById('importPreviewList');
        previewList.innerHTML = pendingImportItems.map(recipe => {
            const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
            return `
                <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0;">
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <b style="color:var(--text-main);">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                        <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                    </div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all;">Ingredients: ${ingList || 'None'}</div>
                </div>
            `;
        }).join('');

        document.getElementById('importPreviewArea').classList.remove('hidden');
        showToast(`Parsed ${pendingImportItems.length} recipes from file`, "info");

    } else {
        // --- INGREDIENT/PART CSV IMPORT (Legacy) ---
        pendingImportType = 'ingredients';

        const nameIdx = findColIdx(['ingredient name', 'ingredientname', 'ingredient', 'name', 'part name', 'partname']);
        const catIdx = findColIdx(['category', 'category name', 'categoryname']);
        const bigUnitIdx = findColIdx(['big unit', 'bigunit', 'big_unit', 'big uom']);
        const smallUnitIdx = findColIdx(['small unit', 'smallunit', 'small_unit', 'small uom']);
        const conversionIdx = findColIdx(['conversion value', 'conversionvalue', 'conversion', 'conversion_value', 'conversion factor']);
        const packQtyIdx = findColIdx(['pack quantity', 'packquantity', 'pack size', 'packsize', 'pack_size']);
        const packPriceIdx = findColIdx(['pack price', 'packprice', 'pack_price', 'purchase price', 'purchase_price', 'cost', 'pack cost']);
        const priceIdx = findColIdx(['selling price', 'price', 'unit price', 'sellingprice', 'unitprice']);
        const stockIdx = findColIdx(['current stock', 'stock', 'quantity', 'qty', 'quantity_in_stock', 'currentstock']);
        const minStockIdx = findColIdx(['minimum stock', 'min stock', 'minimumstock', 'minstock', 'minimum_stock_level']);
        const barcodeIdx = findColIdx(['barcode']);
        const skuIdx = findColIdx(['sku', 'part number', 'partnumber', 'part_number']);
        const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
        const itemNoIdx = findColIdx(['item no', 'itemno', 'no.']);

        if (nameIdx === -1) {
            showToast("Invalid file format. 'Name' or 'Recipe Name' column is required.", "error");
            return;
        }

        pendingImportItems = [];
        for (let i = 1; i < lines.length; i++) {
            const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
            if (cols.length < headers.length) continue;

            const pPrice = packPriceIdx !== -1 ? parseFloat(cols[packPriceIdx]) || 0.00 : (priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.00 : 0.00);

            pendingImportItems.push({
                itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                name: cols[nameIdx] || '',
                category: catIdx !== -1 ? cols[catIdx] : 'General',
                price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || pPrice : pPrice,
                stock: stockIdx !== -1 ? parseFloat(cols[stockIdx]) || 0 : 0,
                barcode: barcodeIdx !== -1 ? cols[barcodeIdx] : '',
                sku: skuIdx !== -1 ? cols[skuIdx] : '',
                description: descIdx !== -1 ? cols[descIdx] : '',
                bigUnit: bigUnitIdx !== -1 ? cols[bigUnitIdx] : '',
                smallUnit: smallUnitIdx !== -1 ? cols[smallUnitIdx] : '',
                conversionValue: conversionIdx !== -1 ? parseFloat(cols[conversionIdx]) || 1.0 : 1.0,
                packSize: packQtyIdx !== -1 ? parseFloat(cols[packQtyIdx]) || 1.0 : 1.0,
                packPrice: pPrice,
                minStock: minStockIdx !== -1 ? parseInt(cols[minStockIdx]) || 5 : 5
            });
        }

        // Show preview
        document.getElementById('importPreviewCount').innerText = pendingImportItems.length + " ingredients";
        const previewList = document.getElementById('importPreviewList');
        previewList.innerHTML = pendingImportItems.map(item => `
            <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr 1fr; gap:10px;">
                <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.name}</b>
                <span style="color:var(--text-muted);">${item.category}</span>
                <span style="color:var(--accent); text-align:right;">$${item.price.toFixed(2)}</span>
                <span style="color:${item.stock === 0 ? 'var(--danger)' : 'var(--text-main)'}; text-align:right;">Qty: ${item.stock}</span>
            </div>
        `).join('');

        document.getElementById('importPreviewArea').classList.remove('hidden');
        showToast(`Parsed ${pendingImportItems.length} items from file`, "info");
    }
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
        const endpoint = pendingImportType === 'recipes' ? 'api/import-recipes' : 'api/import-items';
        const payload = pendingImportType === 'recipes' ? { recipes: pendingImportItems } : { items: pendingImportItems };

        const res = await fetch(`${API_BASE}/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            const result = await res.json();
            if (result.error) {
                showToast(`Failed to import: ${result.error}`, "error");
            } else {
                showToast(`Import Success! Imported: ${result.imported}, Skipped: ${result.skipped}`, "success");
                clearImportPreview();
                await initApp();
            }
        } else {
            showToast("Failed to process import on server", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

async function exportInventoryToCsv() {
    try {
        // Construct CSV Content
        const headers = ["Ingredient Name", "Category", "Big Unit", "Small Unit", "Conversion Value", "Pack Quantity", "Pack Price", "Current Stock", "Minimum Stock", "Cost per Big Unit", "Cost per Small Unit", "SKU", "Barcode", "Item No"];
        const rows = allProducts.map(p => {
            const pSize = parseFloat(p.packSize) || 1.0;
            const conv = parseFloat(p.conversionValue) || 1.0;
            const pPrice = parseFloat(p.packPrice || p.purchasePrice || p.price || 0.00);
            
            const currentStockBigUnit = p.stock * pSize; // Convert stock packs back to Big Unit
            const costPerBigUnit = pPrice / pSize;
            const costPerSmallUnit = costPerBigUnit / conv;

            return [
                `"${p.name.replace(/"/g, '""')}"`,
                `"${(p.category || 'General').replace(/"/g, '""')}"`,
                `"${(p.bigUnit || '').replace(/"/g, '""')}"`,
                `"${(p.smallUnit || '').replace(/"/g, '""')}"`,
                conv,
                pSize,
                pPrice.toFixed(2),
                currentStockBigUnit.toFixed(2).replace(/\.00$/, ''),
                p.minStock || 5,
                costPerBigUnit.toFixed(4),
                costPerSmallUnit.toFixed(4),
                `"${(p.sku || '').replace(/"/g, '""')}"`,
                `"${(p.barcode || '').replace(/"/g, '""')}"`,
                `"${(p.itemNo || '').replace(/"/g, '""')}"`
            ];
        });

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
    const csvContent = "item_no,part_name,category_name,selling_price,quantity_in_stock,barcode,part_number,description\n101,Salmon Fillet,Seafood,15.99,100,72901234567,SF-100,Fresh pink salmon fillet\n102,Tiger Prawns,Seafood,24.50,50,72909876543,TP-200,Frozen tiger prawns large\n";
    
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

// ─── RECIPE IMPORT / EXPORT ──────────────────────────────────────────
let pendingRecipeImportItems = [];

function handleRecipeImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    const isExcel = file.name.endsWith('.xlsx') || file.name.endsWith('.xls');

    if (isExcel) {
        reader.onload = function(evt) {
            try {
                const data = new Uint8Array(evt.target.result);
                const workbook = XLSX.read(data, { type: 'array' });
                const firstSheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[firstSheetName];
                
                // Read sheet to array of arrays
                const rows = XLSX.utils.sheet_to_json(worksheet, { header: 1, defval: "" });
                parseRecipeImportRows(rows);
            } catch (err) {
                console.error(err);
                showToast("Failed to parse Excel file: " + err.message, "error");
            }
        };
        reader.readAsArrayBuffer(file);
    } else {
        reader.onload = function(evt) {
            const content = evt.target.result;
            const isTsv = file.name.endsWith('.tsv') || content.includes('\t');
            const separator = isTsv ? '\t' : ',';
            
            // Parse CSV/TSV into an array of arrays
            const lines = content.split('\n').map(l => l.trim()).filter(l => l.length > 0);
            const rows = lines.map(line => parseCsvLine(line, separator));
            parseRecipeImportRows(rows);
        };
        reader.readAsText(file);
    }
}

function parseCsvLine(text, separator) {
    const result = [];
    let curVal = "";
    let inQuotes = false;
    for (let i = 0; i < text.length; i++) {
        const char = text[i];
        if (char === '"') {
            inQuotes = !inQuotes;
        } else if (char === separator && !inQuotes) {
            result.push(curVal.trim());
            curVal = "";
        } else {
            curVal += char;
        }
    }
    result.push(curVal.trim());
    return result;
}

function parseRecipeImportRows(rows) {
    if (rows.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    const headers = rows[0].map(h => (h || '').toString().trim());

    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const recipeNameIdx = findColIdx(['recipe name', 'recipe_name', 'recipe']);
    if (recipeNameIdx === -1) {
        showToast("Invalid format. 'Recipe Name' column is required.", "error");
        return;
    }

    const catIdx = findColIdx(['category', 'category name', 'categoryname', 'recipe category']);
    const priceIdx = findColIdx(['selling price', 'price', 'recipe price', 'sellingprice']);
    const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
    const itemNoIdx = findColIdx(['item no', 'itemno', 'no.', 'recipe item no']);

    // Find all ingredient / part columns to support duplicate headers
    let ingredientIndices = [];
    headers.forEach((h, idx) => {
        const clean = h.toLowerCase().replace(/[\s_-]/g, '');
        if (clean === 'ingredient' || clean === 'part') {
            ingredientIndices.push(idx);
        }
    });

    let ingNameIdx = -1;
    let ingQtyIdx = -1;

    if (ingredientIndices.length >= 2) {
        ingNameIdx = ingredientIndices[0];
        ingQtyIdx = ingredientIndices[1];
    } else if (ingredientIndices.length === 1) {
        ingNameIdx = ingredientIndices[0];
    }

    if (ingNameIdx === -1) {
        ingNameIdx = findColIdx(['ingredient name', 'ingredientname', 'part name', 'partname', 'part', 'ingredient']);
    }
    if (ingQtyIdx === -1) {
        ingQtyIdx = findColIdx(['ingredient quantity', 'ingredientquantity', 'quantity', 'qty', 'ingredient qty', 'ingredientqty', 'ingredient_quantity', 'quantities', 'amount']);
    }

    const ingUomIdx = findColIdx(['measurement unit', 'measurementunit', 'unit of measure', 'unitofmeasure', 'uom', 'unit']);

    const recipesMap = {};
    for (let i = 1; i < rows.length; i++) {
        const cols = rows[i].map(c => (c === undefined || c === null ? "" : c).toString().trim());
        if (cols.length === 0 || (cols.length === 1 && !cols[0])) continue;

        const rName = cols[recipeNameIdx];
        if (!rName) continue;

        if (!recipesMap[rName]) {
            recipesMap[rName] = {
                name: rName,
                categoryName: catIdx !== -1 ? cols[catIdx] : 'General',
                price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.0 : 0.0,
                description: descIdx !== -1 ? cols[descIdx] : '',
                itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                ingredients: []
            };
        }

        const ingName = ingNameIdx !== -1 ? cols[ingNameIdx] : '';
        const ingQtyVal = ingQtyIdx !== -1 ? cols[ingQtyIdx] : '';
        const ingQty = parseFloat(ingQtyVal) || 1.0;
        const ingUom = ingUomIdx !== -1 ? cols[ingUomIdx] : '';

        if (ingName) {
            recipesMap[rName].ingredients.push({
                name: ingName,
                qty: ingQty,
                unitOfMeasure: ingUom
            });
        }
    }

    pendingRecipeImportItems = Object.values(recipesMap);

    if (pendingRecipeImportItems.length === 0) {
        showToast("No valid recipes found in file", "warn");
        return;
    }

    // Show recipe import preview
    document.getElementById('recipeImportPreviewCount').innerText = pendingRecipeImportItems.length;
    const previewList = document.getElementById('recipeImportPreviewList');
    previewList.innerHTML = pendingRecipeImportItems.map(recipe => {
        const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
        return `
            <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0;">
                <div style="display:flex; justify-content:space-between; align-items:center;">
                    <b style="color:var(--text-main);">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                    <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                </div>
                <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all;">Ingredients: ${ingList || 'None'}</div>
            </div>
        `;
    }).join('');

    document.getElementById('recipeImportPreviewArea').classList.remove('hidden');
    showToast(`Parsed ${pendingRecipeImportItems.length} recipes from file`, "info");
}

async function confirmRecipeImport() {
    if (pendingRecipeImportItems.length === 0) return;
    try {
        const res = await fetch(`${API_BASE}/api/import-recipes`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ recipes: pendingRecipeImportItems })
        });
        if (res.ok) {
            const result = await res.json();
            if (result.error) {
                showToast(`Failed to import: ${result.error}`, "error");
            } else {
                showToast(`Import Success! Imported: ${result.imported}, Skipped: ${result.skipped}`, "success");
                clearRecipeImportPreview();
                await initApp();
            }
        } else {
            showToast("Failed to process import on server", "error");
        }
    } catch (e) { 
        console.error(e);
        showToast("Connection error", "error"); 
    }
}

function clearRecipeImportPreview() {
    pendingRecipeImportItems = [];
    document.getElementById('recipeImportFile').value = '';
    document.getElementById('recipeImportPreviewArea').classList.add('hidden');
    document.getElementById('recipeImportPreviewList').innerHTML = '';
}

function downloadRecipeCsvTemplate() {
    const csvContent = "Recipe Name,Category,Price,Description,Item No,Ingredient,Ingredient,Measurement Unit\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Fish,1,Piece\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Burger Bun,1,Piece\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Lettuce,20,g\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Tartar Sauce,40,g\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Potato Fries,150,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Burger Bun,1,Piece\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Chicken Breast,180,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Lettuce,20,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Tomato,30,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Mayonnaise,25,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Burger Bun,1,Piece\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Beef Patty,200,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Cheddar,25,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Onion,20,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Ketchup,20,g\n";
    
    fetch(`${API_BASE}/api/export-csv`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filename: "Recipes_Template.csv", csvContent })
    })
    .then(res => {
        if (res.ok) showToast("Recipe template downloaded to Downloads folder", "success");
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
    
    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const itemNoIdx = findColIdx(['item no', 'itemno', 'no.']);
    const recipeIdx = findColIdx(['recipe', 'recipe name', 'recipename', 'item', 'item name', 'itemname', 'meal', 'meal name', 'name', 'ingredient', 'ingredient name', 'description']);
    const qtyIdx = findColIdx(['qty sold', 'qtysold', 'qty', 'quantity', 'sold', 'quantity sold', 'count', 'quantity_sold']);
    const unitIdx = findColIdx(['unit', 'uom', 'unit of measure', 'unitofmeasure', 'unit of sale', 'unitofsale']);

    if (recipeIdx === -1 && itemNoIdx === -1) {
        showToast("Invalid file format. 'Item Name' or 'Item No' column is required.", "error");
        return;
    }
    const finalQtyIdx = qtyIdx !== -1 ? qtyIdx : -1;

    pendingSalesItems = [];
    for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
        if (cols.length < headers.length) continue;

        const recipeName = recipeIdx !== -1 ? cols[recipeIdx] || '' : '';
        const qtySold = finalQtyIdx !== -1 ? parseInt(cols[finalQtyIdx]) || 1 : 1;
        const itemNo = itemNoIdx !== -1 ? cols[itemNoIdx] || '' : '';
        const unit = unitIdx !== -1 ? cols[unitIdx] || 'pcs' : 'pcs';

        if (recipeName || itemNo) {
            pendingSalesItems.push({
                itemNo,
                recipeName,
                qtySold,
                unitOfMeasure: unit
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
            <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap; max-width:70%;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.recipeName || 'Unnamed Item'}</b>
            <span style="color:var(--text-main); font-weight:700;">Qty: ${item.qtySold} ${item.unitOfMeasure || ''}</span>
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
                    const partId = d.partId !== undefined ? d.partId : d.PartId;
                    const partName = d.partName || d.PartName || "";
                    const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
                    const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;

                    const p = allProducts.find(x => x.id === partId);

                    let qtyStr = "";
                    let stockStr = "";
                    if (p && p.bigUnit && p.packSize) {
                        const qVal = qtyDeducted * p.packSize;
                        const sVal = newStock * p.packSize;
                        qtyStr = `-${qVal.toFixed(2).replace(/\.00$/, '')} ${p.bigUnit}`;
                        stockStr = `Stock: ${sVal.toFixed(2).replace(/\.00$/, '')} ${p.bigUnit}`;
                    } else {
                        qtyStr = `-${qtyDeducted.toFixed(2).replace(/\.00$/, '')}`;
                        stockStr = `Stock: ${newStock.toFixed(2).replace(/\.00$/, '')}`;
                    }

                    return `
                        <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:6px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr; gap:10px;">
                            <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${partName}</b>
                            <span style="color:var(--danger); text-align:right;">${qtyStr}</span>
                            <span style="color:${newStock <= 0 ? 'var(--danger)' : newStock < 5 ? 'var(--warn)' : 'var(--text-main)'}; text-align:right; font-weight:700;">${stockStr}</span>
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
        const headers = ["Ingredient Name", "Quantity Deducted", "Previous Stock", "New Stock", "Unit"];
        const rows = lastSalesDeductionResults.map(d => {
            const partId = d.partId !== undefined ? d.partId : d.PartId;
            const partName = d.partName || d.PartName || "";
            const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
            const previousStock = d.previousStock !== undefined ? d.previousStock : d.PreviousStock;
            const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;

            const p = allProducts.find(x => x.id === partId);

            let qVal = qtyDeducted;
            let pVal = previousStock;
            let nVal = newStock;
            let unit = "packs";

            if (p && p.bigUnit && p.packSize) {
                qVal = qtyDeducted * p.packSize;
                pVal = previousStock * p.packSize;
                nVal = newStock * p.packSize;
                unit = p.bigUnit;
            }

            return [
                `"${partName.replace(/"/g, '""')}"`,
                qVal.toFixed(4).replace(/\.?0+$/, ''),
                pVal.toFixed(4).replace(/\.?0+$/, ''),
                nVal.toFixed(4).replace(/\.?0+$/, ''),
                `"${unit}"`
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
let reportCharts = {};
window.currentReportData = null;

async function loadReportsData() {
    try {
        const monthSelect = document.getElementById('reportFilterMonth');
        const yearSelect = document.getElementById('reportFilterYear');
        
        // Auto-initialize month/year selects to current date if not set
        if (monthSelect && !monthSelect.dataset.initialized) {
            monthSelect.value = new Date().getMonth() + 1;
            monthSelect.dataset.initialized = 'true';
        }
        if (yearSelect && !yearSelect.dataset.initialized) {
            yearSelect.value = new Date().getFullYear();
            yearSelect.dataset.initialized = 'true';
        }

        const month = monthSelect ? monthSelect.value : (new Date().getMonth() + 1);
        const year = yearSelect ? yearSelect.value : new Date().getFullYear();

        const res = await fetch(`${API_BASE}/api/reports?month=${month}&year=${year}`);
        if (res.ok) {
            const data = await res.json();
            window.currentReportData = data;
            
            // 1. Populate KPI Cards
            document.getElementById('kpiRevenue').innerText = formatPrice(data.revenue || 0);
            document.getElementById('kpiOrders').innerText = data.orders || 0;
            document.getElementById('kpiCOGS').innerText = formatPrice(data.cogs || 0);
            document.getElementById('kpiProfit').innerText = formatPrice(data.grossProfit || 0);
            document.getElementById('kpiMargin').innerText = (data.profitMargin || 0).toFixed(2) + '%';
            document.getElementById('kpiInvValue').innerText = formatPrice(data.financialSummary?.inventoryValue || 0);
            document.getElementById('kpiLowStock').innerText = data.lowStock || 0;
            document.getElementById('kpiOutOfStock').innerText = data.outOfStock || 0;

            // 2. Populate Financial Statements Summary
            document.getElementById('finRevenue').innerText = formatPrice(data.revenue || 0);
            document.getElementById('finPurchaseCost').innerText = formatPrice(data.financialSummary?.purchaseCost || 0);
            document.getElementById('finIngredientCost').innerText = formatPrice(data.cogs || 0);
            document.getElementById('finGrossProfit').innerText = formatPrice(data.grossProfit || 0);
            document.getElementById('finProfitMargin').innerText = (data.profitMargin || 0).toFixed(2) + '%';

            // 3. Render Category Sales List
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

            // 4. Render Recipe Sales Summary Table
            const recipeTbody = document.getElementById('reportsRecipeTableBody');
            if (recipeTbody) {
                recipeTbody.innerHTML = '';
                if (data.recipeSummary && data.recipeSummary.length > 0) {
                    data.recipeSummary.forEach(r => {
                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-name" style="padding:12px; font-weight:600; text-align:left;">${r.name}</td>
                            <td data-label="Quantity Sold" style="padding:12px; text-align:center;">${r.qtySold}</td>
                            <td data-label="Revenue" style="padding:12px; text-align:right; font-weight:bold; color:var(--accent);">${formatPrice(r.revenue)}</td>
                            <td data-label="Cost" style="padding:12px; text-align:right; color:#f87171;">${formatPrice(r.cost)}</td>
                            <td data-label="Profit" style="padding:12px; text-align:right; font-weight:bold; color:#34d399;">${formatPrice(r.profit)}</td>
                        `;
                        recipeTbody.appendChild(tr);
                    });
                } else {
                    recipeTbody.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:20px; color:var(--text-muted);">No recipe sales recorded in this period.</td></tr>';
                }
            }

            // 5. Render Ingredient Consumption Table
            const ingTbody = document.getElementById('reportsIngredientTableBody');
            if (ingTbody) {
                ingTbody.innerHTML = '';
                if (data.ingredientConsumption && data.ingredientConsumption.length > 0) {
                    data.ingredientConsumption.forEach(i => {
                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-name" style="padding:12px; font-weight:600; text-align:left;">${i.name}</td>
                            <td data-label="Opening Stock" style="padding:12px; text-align:center;">${i.openingStock}</td>
                            <td data-label="Purchased / Added" style="padding:12px; text-align:center; color:#34d399;">+${i.purchased}</td>
                            <td data-label="Used in Recipes" style="padding:12px; text-align:center; color:#f87171;">-${i.usedInRecipes}</td>
                            <td data-label="Sold Directly" style="padding:12px; text-align:center; color:#fbbf24;">-${i.soldDirectly}</td>
                            <td data-label="Closing Stock" style="padding:12px; text-align:center; font-weight:bold;">${i.closingStock}</td>
                            <td data-label="Unit" style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">${i.unit}</td>
                        `;
                        ingTbody.appendChild(tr);
                    });
                } else {
                    ingTbody.innerHTML = '<tr><td colspan="7" style="text-align:center; padding:20px; color:var(--text-muted);">No ingredient logs found.</td></tr>';
                }
            }

            // 6. Render Stock Movement History Table
            const movTbody = document.getElementById('reportsMovementTableBody');
            if (movTbody) {
                movTbody.innerHTML = '';
                if (data.stockMovement && data.stockMovement.length > 0) {
                    data.stockMovement.forEach(m => {
                        let typeColor = 'inherit';
                        let prefix = '';
                        if (m.type === 'Purchase') { typeColor = '#34d399'; prefix = '+'; }
                        else if (m.type === 'Ingredient Sale' || m.type === 'Recipe Consumption') { typeColor = '#f87171'; prefix = '-'; }
                        else if (m.type.includes('Adjustment')) { typeColor = '#fbbf24'; prefix = m.quantity < 0 ? '-' : '+'; }

                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-date" style="padding:12px; text-align:left; font-size:0.85rem; color:var(--text-muted);">${m.date}</td>
                            <td data-label="Ingredient" style="padding:12px; font-weight:600; text-align:left;">${m.ingredient}</td>
                            <td data-label="Movement Type" style="padding:12px; text-align:center; font-weight:bold; color:${typeColor};">${m.type}</td>
                            <td data-label="Quantity Change" style="padding:12px; text-align:center; font-weight:bold; color:${typeColor};">${prefix}${Math.abs(m.quantity)}</td>
                            <td data-label="Unit" style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">${m.unit}</td>
                            <td data-label="Remaining Stock" style="padding:12px; text-align:center; font-weight:bold;">${m.remainingStock}</td>
                        `;
                        movTbody.appendChild(tr);
                    });
                } else {
                    movTbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">No stock movements recorded in this period.</td></tr>';
                }
            }

            // 7. Render Charts
            if (data.chartsData) {
                renderReportCharts(data.chartsData);
            }
        }
    } catch (e) { console.error("Failed to load reports", e); }
}

function renderReportCharts(chartsData) {
    try {
        const isDarkMode = document.body.classList.contains('dark-mode');
        const themeColorText = isDarkMode ? 'rgba(255, 255, 255, 0.7)' : 'rgba(30, 41, 59, 0.85)';
        const themeColorGrid = isDarkMode ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.08)';

        // Destroy existing charts to prevent overlaps
        if (reportCharts.salesDay) reportCharts.salesDay.destroy();
        if (reportCharts.revTrend) reportCharts.revTrend.destroy();
        if (reportCharts.bestRecipes) reportCharts.bestRecipes.destroy();
        if (reportCharts.mostIngredients) reportCharts.mostIngredients.destroy();

        const commonOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { labels: { color: themeColorText, font: { family: 'Inter', weight: 'bold' } } }
            },
            scales: {
                x: { grid: { color: themeColorGrid }, ticks: { color: themeColorText } },
                y: { grid: { color: themeColorGrid }, ticks: { color: themeColorText } }
            }
        };

        // 1. Sales by Day
        const ctxSales = document.getElementById('chartSalesDay')?.getContext('2d');
        if (ctxSales) {
            const labels = Object.keys(chartsData.salesByDay || {});
            const data = Object.values(chartsData.salesByDay || {});
            reportCharts.salesDay = new Chart(ctxSales, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Daily Sales ($)',
                        data: data,
                        borderColor: '#06b6d4',
                        backgroundColor: 'rgba(6, 182, 212, 0.1)',
                        fill: true,
                        tension: 0.3,
                        borderWidth: 3
                    }]
                },
                options: commonOptions
            });
        }

        // 2. Monthly Revenue Trend
        const ctxTrend = document.getElementById('chartRevTrend')?.getContext('2d');
        if (ctxTrend) {
            const labels = (chartsData.revenueTrend || []).map(x => x.month);
            const data = (chartsData.revenueTrend || []).map(x => x.revenue);
            reportCharts.revTrend = new Chart(ctxTrend, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Monthly Revenue Trend ($)',
                        data: data,
                        borderColor: '#10b981',
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        fill: true,
                        tension: 0.2,
                        borderWidth: 3
                    }]
                },
                options: commonOptions
            });
        }

        // 3. Best Selling Recipes
        const ctxRecipes = document.getElementById('chartBestRecipes')?.getContext('2d');
        if (ctxRecipes) {
            const labels = (chartsData.bestRecipes || []).map(x => x.name);
            const data = (chartsData.bestRecipes || []).map(x => x.qty);
            reportCharts.bestRecipes = new Chart(ctxRecipes, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Portions Sold',
                        data: data,
                        backgroundColor: '#3b82f6',
                        borderRadius: 6
                    }]
                },
                options: commonOptions
            });
        }

        // 4. Most Consumed Ingredients
        const ctxIngredients = document.getElementById('chartMostIngredients')?.getContext('2d');
        if (ctxIngredients) {
            const labels = (chartsData.mostIngredients || []).map(x => x.name);
            const data = (chartsData.mostIngredients || []).map(x => x.qty);
            reportCharts.mostIngredients = new Chart(ctxIngredients, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: data,
                        backgroundColor: ['#06b6d4', '#10b981', '#3b82f6', '#fbbf24', '#f87171'],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right', labels: { color: themeColorText } }
                    }
                }
            });
        }
    } catch (err) { console.error("Failed to render Chart.js graphs", err); }
}

window.switchReportSubtab = function(subtabId) {
    const panels = document.querySelectorAll('.reports-panel');
    panels.forEach(p => p.classList.add('hidden'));

    const selectedPanel = document.getElementById(`panelReports${subtabId.charAt(0).toUpperCase() + subtabId.slice(1)}`);
    if (selectedPanel) selectedPanel.classList.remove('hidden');

    const tabButtons = document.querySelectorAll('.btn-subtab');
    tabButtons.forEach(btn => btn.classList.remove('active'));

    const activeBtn = document.getElementById(`btnSubtab${subtabId.charAt(0).toUpperCase() + subtabId.slice(1)}`);
    if (activeBtn) activeBtn.classList.add('active');
};

window.exportReport = async function(format) {
    const data = window.currentReportData;
    if (!data) {
        showToast("No report data available to export.", "error");
        return;
    }

    const monthSelect = document.getElementById('reportFilterMonth');
    const yearSelect = document.getElementById('reportFilterYear');
    const monthName = monthSelect.options[monthSelect.selectedIndex].text;
    const yearVal = yearSelect.value;

    if (format === 'pdf') {
        const periodEl = document.getElementById('printReportPeriod');
        if (periodEl) periodEl.innerText = `Selected Period: ${monthName} ${yearVal}`;
        if (window.AndroidBridge && typeof window.AndroidBridge.printPage === 'function') {
            window.AndroidBridge.printPage(`monthly_report_${monthName.replace(/\s+/g, '_')}_${yearVal}`);
        } else {
            window.print();
        }
        return;
    }

    try {
        const filename = `monthly_report_${monthName.replace(/\s+/g, '_')}_${yearVal}.csv`;
        let csv = "";
        
        // 1. Title
        csv += `MONTHLY BUSINESS ACTIVITY REPORT - ${monthName.toUpperCase()} ${yearVal}\n\n`;

        // 2. Financial Summary Section
        csv += "FINANCIAL SUMMARY\n";
        csv += `Total Sales Revenue,${data.revenue.toFixed(2)}\n`;
        csv += `Total Sales Count,${data.orders}\n`;
        csv += `Cost of Goods Sold (COGS),${data.cogs.toFixed(2)}\n`;
        csv += `Gross Profit,${data.grossProfit.toFixed(2)}\n`;
        csv += `Profit Margin,${(data.profitMargin || 0).toFixed(2)}%\n`;
        csv += `Inventory Asset Value,${(data.financialSummary?.inventoryValue || 0).toFixed(2)}\n\n`;

        // 3. Recipe Summary Table
        csv += "RECIPE SALES SUMMARY\n";
        csv += "Recipe Name,Quantity Sold,Revenue,Cost of Ingredients,Net Profit\n";
        if (data.recipeSummary && data.recipeSummary.length > 0) {
            data.recipeSummary.forEach(r => {
                csv += `"${r.name}",${r.qtySold},${r.revenue.toFixed(2)},${r.cost.toFixed(2)},${r.profit.toFixed(2)}\n`;
            });
        } else {
            csv += "No records found\n";
        }
        csv += "\n";

        // 4. Ingredient Consumption Table
        csv += "INGREDIENT CONSUMPTION\n";
        csv += "Ingredient Name,Opening Stock,Purchased Qty,Quantity Used in Recipes,Quantity Sold Directly,Closing Stock,Unit\n";
        if (data.ingredientConsumption && data.ingredientConsumption.length > 0) {
            data.ingredientConsumption.forEach(i => {
                csv += `"${i.name}",${i.openingStock},${i.purchased},${i.usedInRecipes},${i.soldDirectly},${i.closingStock},"${i.unit}"\n`;
            });
        } else {
            csv += "No records found\n";
        }
        csv += "\n";

        // 5. Stock Movement History Table
        csv += "STOCK MOVEMENT HISTORY\n";
        csv += "Date & Time,Ingredient,Movement Type,Quantity Change,Unit,Remaining Stock\n";
        if (data.stockMovement && data.stockMovement.length > 0) {
            data.stockMovement.forEach(m => {
                csv += `"${m.date}","${m.ingredient}","${m.type}",${m.quantity},"${m.unit}",${m.remainingStock}\n`;
            });
        } else {
            csv += "No records found\n";
        }

        // Export call (Android WebView or browser download fallback)
        if (window.AndroidBridge || API_BASE !== '') {
            const res = await fetch(`${API_BASE}/api/export-csv`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ filename, csvContent: csv })
            });
            if (res.ok) {
                const resData = await res.json();
                showToast(`Report exported successfully to ${resData.path}!`, "success");
            } else {
                showToast("Failed to export on device.", "error");
            }
        } else {
            const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement("a");
            const url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            showToast("Report spreadsheet downloaded successfully!", "success");
        }
    } catch (e) {
        showToast("Error exporting report file.", "error");
        console.error(e);
    }
};

window.clearReportsData = function() {
    showDeleteConfirm(
        "Clear Daily Report",
        "Are you sure you want to clear all sales orders and activity logs to start a fresh daily report? This action cannot be undone.",
        async () => {
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
        },
        "Clear"
    );
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


// ─── MANUAL SALES ENTRY TAB ───────────────────────────────────────────
window.loadSalesTab = async function() {
    // 1. Populate product & recipe dropdown with optgroups
    const select = document.getElementById('salesProductSelect');
    if (select) {
        // Keep initial option
        select.innerHTML = '<option value="">Select an item...</option>';
        
        // Products group
        const prodGroup = document.createElement('optgroup');
        prodGroup.label = "Ingredients & Products";
        const sortedProducts = [...allProducts].sort((a, b) => a.name.localeCompare(b.name));
        sortedProducts.forEach(p => {
            if (p.status !== 'Inactive') {
                const opt = document.createElement('option');
                opt.value = `prod-${p.id}`;
                opt.textContent = `${p.name} ($${p.price.toFixed(2)})`;
                prodGroup.appendChild(opt);
            }
        });
        select.appendChild(prodGroup);

        // Recipes group
        const recGroup = document.createElement('optgroup');
        recGroup.label = "Food Recipes";
        const sortedRecipes = [...allRecipes].sort((a, b) => a.name.localeCompare(b.name));
        sortedRecipes.forEach(r => {
            if (r.status !== 'Inactive') {
                const opt = document.createElement('option');
                opt.value = `rec-${r.id}`;
                opt.textContent = `${r.name} ($${r.price.toFixed(2)})`;
                recGroup.appendChild(opt);
            }
        });
        select.appendChild(recGroup);
    }

    // 2. Fetch and render sales items table
    try {
        const res = await fetch(`${API_BASE}/api/sales-items`);
        if (res.ok) {
            const items = await res.json();
            renderSalesItemsTable(items);
        } else {
            showToast("Failed to fetch sales history", "error");
        }
    } catch (e) {
        console.error("Failed to load sales items", e);
    }
    
    // Reset Form
    if (select) select.value = "";
    const qtyInput = document.getElementById('salesQuantityInput');
    if (qtyInput) qtyInput.value = "1";
    const priceInput = document.getElementById('salesPriceInput');
    if (priceInput) priceInput.value = "0.00";
    const uomGroup = document.getElementById('salesUomGroup');
    if (uomGroup) uomGroup.style.display = 'none';
    calculateSalesTotal();
};

window.onSalesProductChange = function() {
    const select = document.getElementById('salesProductSelect');
    const priceInput = document.getElementById('salesPriceInput');
    const uomGroup = document.getElementById('salesUomGroup');
    const uomSelect = document.getElementById('salesUomSelect');
    if (!select || !priceInput) return;

    if (!select.value) {
        priceInput.value = "0.00";
        if (uomGroup) uomGroup.style.display = 'none';
        calculateSalesTotal();
        return;
    }

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);

    if (type === 'prod') {
        const product = allProducts.find(p => p.id === itemId);
        if (product) {
            priceInput.value = product.price.toFixed(2);
            if (product.bigUnit && product.smallUnit && uomSelect && uomGroup) {
                uomSelect.innerHTML = `
                    <option value="pack">pack</option>
                    <option value="${product.bigUnit.toLowerCase()}">${product.bigUnit}</option>
                    <option value="${product.smallUnit.toLowerCase()}">${product.smallUnit}</option>
                `;
                uomGroup.style.display = 'block';
            } else {
                if (uomGroup) uomGroup.style.display = 'none';
            }
        }
    } else if (type === 'rec') {
        const recipe = allRecipes.find(r => r.id === itemId);
        if (recipe) {
            priceInput.value = recipe.price.toFixed(2);
        }
        if (uomGroup) uomGroup.style.display = 'none';
    }
    calculateSalesTotal();
};

window.onSalesUomChange = function() {
    const select = document.getElementById('salesProductSelect');
    const uomSelect = document.getElementById('salesUomSelect');
    const priceInput = document.getElementById('salesPriceInput');
    if (!select || !uomSelect || !priceInput) return;

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);
    if (type !== 'prod') return;

    const product = allProducts.find(p => p.id === itemId);
    if (!product) return;

    const val = uomSelect.value;
    const basePrice = product.price;
    const convVal = parseFloat(product.conversionValue) || 1.0;
    const packSz = parseFloat(product.packSize) || 1.0;

    if (val === 'pack') {
        priceInput.value = basePrice.toFixed(2);
    } else if (val === product.bigUnit.toLowerCase()) {
        priceInput.value = (basePrice / packSz).toFixed(4);
    } else if (val === product.smallUnit.toLowerCase()) {
        priceInput.value = ((basePrice / packSz) / convVal).toFixed(5);
    }
    calculateSalesTotal();
};

window.calculateSalesTotal = function() {
    const qtyInput = document.getElementById('salesQuantityInput');
    const priceInput = document.getElementById('salesPriceInput');
    const totalDisplay = document.getElementById('salesTotalDisplay');
    if (!qtyInput || !priceInput || !totalDisplay) return;

    const qty = parseFloat(qtyInput.value) || 0;
    const price = parseFloat(priceInput.value) || 0;
    const total = qty * price;
    totalDisplay.textContent = `$${total.toFixed(2)}`;
};

function renderSalesItemsTable(items) {
    const tbody = document.getElementById('salesTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!items || items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">No sales entries found.</td></tr>';
        return;
    }

    items.forEach(item => {
        const tr = document.createElement('tr');
        const formattedDate = new Date(item.date).toLocaleString();
        
        tr.innerHTML = `
            <td style="padding:12px; border-bottom:1px solid var(--border); color:var(--text-main); font-weight:600;">${item.name}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right;">${item.qty} ${item.unitOfMeasure || ''}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right;">$${item.price.toFixed(2)}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right; font-weight:700; color:var(--accent);">$${item.total.toFixed(2)}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:center; color:var(--text-muted); font-size:0.85rem;">${formattedDate}</td>
        `;
        tbody.appendChild(tr);
    });
}

window.submitSalesEntry = async function() {
    const select = document.getElementById('salesProductSelect');
    const qtyInput = document.getElementById('salesQuantityInput');
    const priceInput = document.getElementById('salesPriceInput');
    if (!select || !qtyInput || !priceInput) return;

    if (!select.value) {
        showToast("Please select an item", "error");
        return;
    }

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);
    const qty = parseFloat(qtyInput.value);
    const price = parseFloat(priceInput.value);

    if (!qty || qty <= 0) {
        showToast("Please enter a valid quantity", "error");
        return;
    }
    if (isNaN(price) || price <= 0) {
        showToast("Please enter a valid price", "error");
        return;
    }

    let payloadItem = null;

    if (type === 'prod') {
        const product = allProducts.find(p => p.id === itemId);
        if (!product) return;

        // Check stock if tracked
        if (product.isStockTracked && qty > product.stock) {
            showToast(`Insufficient stock! Current stock: ${product.stock}`, "error");
            return;
        }

        const uomSelect = document.getElementById('salesUomSelect');
        const selectedUom = (product.bigUnit && product.smallUnit && uomSelect) ? uomSelect.value : (product.unitOfMeasure || 'pcs');

        payloadItem = {
            id: itemId,
            name: product.name,
            price: price,
            qty: qty,
            itemType: "Part"
        };
    } else if (type === 'rec') {
        const recipe = allRecipes.find(r => r.id === itemId);
        if (!recipe) return;

        payloadItem = {
            id: itemId,
            name: recipe.name,
            price: price,
            qty: qty,
            itemType: "Recipe",
            recipeId: itemId
        };
    }

    if (!payloadItem) return;

    const payload = {
        items: [payloadItem]
    };

    try {
        const res = await fetch(`${API_BASE}/api/checkout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast("Sale recorded successfully!", "success");
            
            // Re-fetch products to update cached stock count
            await fetchInventory();
            
            // Reload the sales tab dropdown and table
            await loadSalesTab();
        } else {
            const errText = await res.text();
            showToast("Checkout failed: " + errText, "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

window.clearSalesHistory = function() {
    showDeleteConfirm(
        "Clear Sales History",
        "Are you sure you want to clear all sales history and transaction logs? This action cannot be undone.",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/clear-reports`, { method: 'POST' });
                if (res.ok) {
                    showToast("Sales history cleared successfully!", "success");
                    await fetchInventory();
                    await loadSalesTab();
                } else {
                    showToast("Failed to clear sales history", "error");
                }
            } catch (e) {
                showToast("Connection error", "error");
            }
        },
        "Clear"
    );
};

// ─── STOCK MENU MANAGEMENT ───────────────────────────────────────────
function getTodayDateString() {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

window.loadStockTab = function() {
    const dateInput = document.getElementById('stockReportDate');
    if (dateInput && !dateInput.value) {
        dateInput.value = getTodayDateString();
    }
    loadStockTabList();
    loadStockReport();
};

window.loadStockTabList = function() {
    const container = document.getElementById('stockTabCardsContainer');
    if (!container) return;
    container.innerHTML = '';

    const query = document.getElementById('stockTabSearch')?.value.toLowerCase() || '';
    
    const filtered = allProducts.filter(p => {
        return !query || 
            (p.name && p.name.toLowerCase().includes(query)) || 
            (p.sku && p.sku.toLowerCase().includes(query)) || 
            (p.barcode && p.barcode.toLowerCase().includes(query));
    });

    if (filtered.length === 0) {
        container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--text-muted); font-weight:600; font-size:0.95rem;">No products found.</div>`;
        return;
    }

    filtered.forEach(p => {
        const card = document.createElement('div');
        card.className = 'glass-card';
        card.style.padding = '14px';
        card.style.display = 'flex';
        card.style.justifyContent = 'space-between';
        card.style.alignItems = 'center';
        card.style.border = '1px solid rgba(255,255,255,0.08)';
        card.style.background = 'rgba(255,255,255,0.03)';
        card.style.borderRadius = '10px';
        
        const unitToShow = p.bigUnit || p.unitOfMeasure || 'pcs';
        const stockInBigUnit = (p.bigUnit && p.packSize) ? (p.stock * p.packSize) : p.stock;
        const stockVal = stockInBigUnit.toFixed(2).replace(/\.00$/, '');

        const hasSmallUnit = p.smallUnit && p.conversionValue > 0;
        const unitBadge = hasSmallUnit ? 
            `<span id="quickAdjustUnit_${p.id}" data-unit-type="big" onclick="toggleQuickAdjustUnit(${p.id}, '${unitToShow}', '${p.smallUnit}')" style="font-size:0.85rem; color:var(--accent); font-weight:700; cursor:pointer; border-bottom:1px dashed var(--accent); max-width:60px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; transition:all 0.2s;">${unitToShow}</span>` :
            `<span style="font-size:0.85rem; color:var(--text-muted); font-weight:600; max-width:60px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${unitToShow}</span>`;

        card.innerHTML = `
            <div>
                <div style="font-weight:bold; font-size:1rem; color:var(--text-main);">${p.name}</div>
                <div style="font-size:0.85rem; color:var(--text-muted); margin-top:4px;">
                    Current Stock: <span style="font-weight:bold; color:${p.stock === 0 ? 'var(--danger)' : 'var(--accent)'};">${stockVal} ${unitToShow}</span>
                </div>
            </div>
            <div style="display:flex; align-items:center; gap:8px;">
                <button onclick="adjustStockItemWithInput(${p.id}, -1)" style="width:36px; height:36px; border-radius:8px; border:none; background:rgba(239, 68, 68, 0.2); color:#ef4444; font-weight:bold; font-size:1.2rem; cursor:pointer; display:flex; align-items:center; justify-content:center;">-</button>
                <div style="display:flex; align-items:center; justify-content:center; gap:4px; background:rgba(0,0,0,0.15); padding:2px 8px; border-radius:6px; border:1px solid rgba(255,255,255,0.1); width:120px; box-sizing:border-box;">
                    <input type="number" id="quickAdjustInput_${p.id}" value="1" min="0.001" step="any" style="width:40px; border:none; background:transparent; color:white; text-align:center; font-size:0.9rem; font-weight:bold; outline:none;" />
                    ${unitBadge}
                </div>
                <button onclick="adjustStockItemWithInput(${p.id}, 1)" style="width:36px; height:36px; border-radius:8px; border:none; background:rgba(16, 185, 129, 0.2); color:#10b981; font-weight:bold; font-size:1.2rem; cursor:pointer; display:flex; align-items:center; justify-content:center;">+</button>
            </div>
        `;
        container.appendChild(card);
    });
};

window.toggleQuickAdjustUnit = function(productId, bigUnit, smallUnit) {
    if (!smallUnit) return;
    const badge = document.getElementById(`quickAdjustUnit_${productId}`);
    if (!badge) return;
    
    const current = badge.getAttribute('data-unit-type');
    if (current === 'big') {
        badge.setAttribute('data-unit-type', 'small');
        badge.innerText = smallUnit;
        badge.style.color = '#f59e0b'; // amber color for small unit
        badge.style.borderBottomColor = '#f59e0b';
    } else {
        badge.setAttribute('data-unit-type', 'big');
        badge.innerText = bigUnit;
        badge.style.color = 'var(--accent)';
        badge.style.borderBottomColor = 'var(--accent)';
    }
};

window.filterStockTabList = function() {
    loadStockTabList();
};

window.adjustStockItemWithInput = async function(productId, direction) {
    const input = document.getElementById(`quickAdjustInput_${productId}`);
    if (!input) return;
    const value = parseFloat(input.value);
    if (isNaN(value) || value <= 0) {
        showToast("Please enter a valid positive number", "error");
        return;
    }
    
    const p = allProducts.find(x => x.id === productId);
    if (!p) return;

    const badge = document.getElementById(`quickAdjustUnit_${productId}`);
    const unitType = badge ? badge.getAttribute('data-unit-type') : 'big';

    let delta = direction * value;
    
    // Scale down small unit to big unit base first if selected
    if (unitType === 'small' && p.conversionValue > 0) {
        delta = delta / p.conversionValue;
    }
    
    // Scale delta (Big Unit adjustment) to packs for database update
    const dbChange = (p.bigUnit && p.packSize) ? (delta / p.packSize) : delta;
    
    await adjustStockItem(productId, dbChange);
};

window.adjustStockItem = async function(productId, delta) {
    try {
        const res = await fetch(`${API_BASE}/api/adjust-stock`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ partId: productId, change: delta, reason: 'Daily Quick Adjustment' })
        });

        if (res.ok) {
            showToast("Stock updated!", "success");
            await fetchInventory();
            loadStockTabList();
            loadStockReport();
        } else {
            showToast("Failed to update stock", "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

window.loadStockReport = async function() {
    const container = document.getElementById('stockReportCardsContainer');
    if (!container) return;
    container.innerHTML = '';

    const selectedDate = document.getElementById('stockReportDate').value;
    if (!selectedDate) return;

    try {
        const res = await fetch(`${API_BASE}/api/stock-transactions?date=${selectedDate}`);
        if (res.ok) {
            const list = await res.json();
            if (list.length === 0) {
                container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--text-muted); font-weight:600; font-size:0.95rem;">No stock movements recorded for this day.</div>`;
                return;
            }

            list.forEach(tx => {
                const card = document.createElement('div');
                card.className = 'glass-card';
                card.style.padding = '14px';
                card.style.display = 'flex';
                card.style.justifyContent = 'space-between';
                card.style.alignItems = 'center';
                card.style.border = '1px solid rgba(255,255,255,0.08)';
                card.style.background = 'rgba(255,255,255,0.03)';
                card.style.borderRadius = '10px';
                
                let timeStr = tx.time;
                if (tx.time && tx.time.includes(' ')) {
                    timeStr = tx.time.split(' ')[1].substring(0, 5);
                } else if (tx.time && tx.time.includes('T')) {
                    timeStr = tx.time.split('T')[1].substring(0, 5);
                }

                // Dynamic unit lookup from allProducts array
                const p = allProducts.find(x => x.name === tx.item);
                const unitToShow = p ? (p.bigUnit || p.unitOfMeasure || 'pcs') : 'pcs';

                // Try to extract the quantity change value from description
                let qty = 0;
                let hasQty = false;
                let match = tx.desc.match(/by\s+(-?\d+(\.\d+)?)/i);
                if (match) {
                    qty = parseFloat(match[1]);
                    hasQty = true;
                } else {
                    match = tx.desc.match(/(?:New\s+)?Qty:\s*(-?\d+(\.\d+)?)/i);
                    if (match) {
                        qty = parseFloat(match[1]);
                        hasQty = true;
                    }
                }

                let badgeBg = 'rgba(255, 255, 255, 0.1)';
                let badgeColor = '#fff';
                let actionText = 'EDIT';
                
                if (tx.action === 'ADJUST_IN' || tx.action === 'STOCK_ADD') {
                    badgeBg = 'rgba(16, 185, 129, 0.15)';
                    badgeColor = '#10b981';
                    actionText = 'ADDED';
                } else if (tx.action === 'ADJUST_OUT') {
                    badgeBg = 'rgba(239, 68, 68, 0.15)';
                    badgeColor = '#ef4444';
                    actionText = 'REMOVED';
                } else if (tx.action === 'STOCK_EDIT') {
                    badgeBg = 'rgba(59, 130, 246, 0.15)';
                    badgeColor = '#3b82f6';
                    actionText = 'EDIT';
                }

                let qtyStr = '';
                if (hasQty) {
                    if (actionText === 'REMOVED' && qty > 0) {
                        qtyStr = `-${qty.toFixed(2).replace(/\.00$/, '')}`;
                    } else if (actionText === 'ADDED' && qty > 0) {
                        qtyStr = `+${qty.toFixed(2).replace(/\.00$/, '')}`;
                    } else {
                        qtyStr = qty > 0 ? `+${qty.toFixed(2).replace(/\.00$/, '')}` : qty.toFixed(2).replace(/\.00$/, '');
                    }
                } else {
                    qtyStr = '1';
                }

                card.innerHTML = `
                    <div>
                        <div style="font-size:0.8rem; color:var(--text-muted); font-weight:600; margin-bottom:4px;">${timeStr}</div>
                        <div style="font-weight:bold; font-size:0.95rem; color:var(--text-main);">${tx.item}</div>
                    </div>
                    <div style="text-align:right;">
                        <div style="font-size:1rem; font-weight:bold; color:${badgeColor};">${qtyStr} ${unitToShow}</div>
                        <span style="display:inline-block; margin-top:4px; padding:2px 6px; border-radius:4px; font-size:0.7rem; font-weight:bold; background:${badgeBg}; color:${badgeColor};">${actionText}</span>
                    </div>
                `;
                container.appendChild(card);
            });
        } else {
            container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--danger); font-weight:600; font-size:0.95rem;">Failed to load movements.</div>`;
        }
    } catch (e) {
        container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--danger); font-weight:600; font-size:0.95rem;">Connection error.</div>`;
    }
};

// ─── ICON AND IMAGE PICKER LOGIC ──────────────────────────────────────────
const PRESET_ICONS = [
    // Seafood
    '🐟', '🐠', '🐡', '🦈', '🐙', '🐚', '🦀', '🦞', '🦐', '🦑',
    // Meals
    '🍲', '🥗', '🍱', '🍛', '🍣', '🍙', '🍘', '🍝', '🍜', '🍢', '🍤', '🍳', '🥞', '🥩', '🍗', '🌭', '🍔', '🍟', '🍕',
    // Groceries / Vegetables
    '🥚', '🥛', '🧈', '🧅', '🥔', '🥕', '🌽', '🌶️', '🫑', '🥒', '🥬', '🥦', '🧄', '🍄', '🍋', '🍌', '🍎', '🍐', '🍑', '🍒', '🍓', '🫐', '🍇', '🥑',
    // Drinks / Sweets
    '🥤', '🧃', '🧉', '🍵', '☕', '🍺', '🍷', '🍧', '🍨', '🍦', '🍰', '🧁', '🍮', '🍩', '🍪',
    // General
    '📦', '🏷️', '🍽️', '🛒', '🛍️', '✨'
];

window.openIconPickerFor = function(target) {
    iconPickerTarget = target;
    
    // Set title and labels according to page language
    document.getElementById('iconPickerModal').querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });

    const grid = document.getElementById('presetIconsGrid');
    if (grid && grid.children.length === 0) {
        PRESET_ICONS.forEach(emoji => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.style.cssText = 'background: rgba(255,255,255,0.05); border: 1px solid var(--border-color); border-radius: 8px; font-size: 1.8rem; cursor: pointer; aspect-ratio: 1; display: flex; align-items: center; justify-content: center; padding: 5px; transition: all 0.2s;';
            btn.innerText = emoji;
            btn.onclick = () => selectPresetIcon(emoji);
            
            // Hover effect
            btn.onmouseover = () => btn.style.background = 'rgba(255,255,255,0.15)';
            btn.onmouseout = () => btn.style.background = 'rgba(255,255,255,0.05)';
            
            grid.appendChild(btn);
        });
    }

    document.getElementById('iconPickerModal').classList.remove('hidden');
};

window.closeIconPickerModal = function() {
    document.getElementById('iconPickerModal').classList.add('hidden');
};

window.triggerDeviceGallery = function() {
    document.getElementById('iconFileInput').click();
};

function selectPresetIcon(emoji) {
    if (iconPickerTarget === 'product') {
        selectedProductIcon = emoji;
        updateIconPreview('product', emoji);
    } else {
        selectedRecipeIcon = emoji;
        updateIconPreview('recipe', emoji);
    }
    closeIconPickerModal();
}

function isImagePath(val) {
    if (!val) return false;
    if (val.startsWith('data:') || val.startsWith('http:') || val.startsWith('https:') || val.includes('/') || val.includes('\\')) {
        return true;
    }
    const lower = val.toLowerCase();
    return lower.includes('.png') || lower.includes('.jpg') || lower.includes('.jpeg') || 
           lower.includes('.gif') || lower.includes('.svg') || lower.includes('.webp') || lower.includes('.ico');
}

function updateIconPreview(type, val) {
    const previewDiv = document.getElementById(type === 'product' ? 'productIconPreview' : 'recipeIconPreview');
    if (!previewDiv) return;
    if (val && isImagePath(val)) {
        previewDiv.innerHTML = `<img src="${val}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 10px;">`;
    } else {
        previewDiv.innerHTML = val || (type === 'product' ? '📦' : '🍲');
    }
}

