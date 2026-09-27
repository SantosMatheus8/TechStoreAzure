let currentPage = 1;
const pageSize = 10;
let searchTimeout = null;

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    loadProducts();
    setupSearch();
    checkUrlMessage();
});

function checkUrlMessage() {
    const params = new URLSearchParams(window.location.search);
    const msg = params.get('msg');
    if (msg) {
        showAlert(msg, 'success');
        window.history.replaceState({}, '', 'index.html');
    }
}

function setupSearch() {
    document.getElementById('search-input').addEventListener('input', () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            currentPage = 1;
            loadProducts();
        }, 400);
    });

    document.getElementById('category-filter').addEventListener('change', () => {
        currentPage = 1;
        loadProducts();
    });
}

async function loadCategories() {
    try {
        const res = await fetch(`${API_BASE_URL}/api/categorias?pageSize=50`);
        if (!res.ok) throw new Error('Erro ao carregar categorias');
        const data = await res.json();
        const select = document.getElementById('category-filter');
        data.items.forEach(cat => {
            if (!cat.ativo) return;
            const opt = document.createElement('option');
            opt.value = cat.id;
            opt.textContent = cat.nome;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error('Erro ao carregar categorias:', err);
    }
}

async function loadProducts() {
    const loading = document.getElementById('loading');
    const table = document.getElementById('products-table');
    const empty = document.getElementById('empty-state');

    loading.style.display = 'block';
    table.style.display = 'none';
    empty.style.display = 'none';

    const nome = document.getElementById('search-input').value.trim();
    const categoriaId = document.getElementById('category-filter').value;

    let url = `${API_BASE_URL}/api/produtos?page=${currentPage}&pageSize=${pageSize}`;
    if (nome) url += `&nome=${encodeURIComponent(nome)}`;
    if (categoriaId) url += `&categoriaId=${categoriaId}`;

    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error('Erro ao carregar produtos');
        const data = await res.json();

        loading.style.display = 'none';

        if (data.items.length === 0) {
            empty.style.display = 'block';
            return;
        }

        table.style.display = 'block';
        renderProducts(data.items);
        renderPagination(data);
    } catch (err) {
        loading.style.display = 'none';
        showAlert('Falha ao conectar com a API. Verifique se o servidor está rodando.', 'error');
    }
}

function renderProducts(products) {
    const tbody = document.getElementById('products-body');
    tbody.innerHTML = products.map(p => `
        <tr>
            <td>
                <strong>${escapeHtml(p.nome)}</strong>
                <br><small style="color: var(--text-muted)">${escapeHtml(p.descricao || '').substring(0, 60)}</small>
            </td>
            <td>${escapeHtml(p.categoriaNome || '-')}</td>
            <td class="price">R$ ${p.preco.toFixed(2).replace('.', ',')}</td>
            <td>${p.estoque}</td>
            <td>
                <span class="badge ${p.ativo ? 'badge-active' : 'badge-inactive'}">
                    ${p.ativo ? 'Ativo' : 'Inativo'}
                </span>
            </td>
            <td>
                <a href="produto-form.html?id=${p.id}" class="btn btn-outline btn-sm">Editar</a>
                <button class="btn btn-danger btn-sm" onclick="openDeleteModal('${p.id}', '${escapeHtml(p.nome)}')">Excluir</button>
            </td>
        </tr>
    `).join('');
}

function renderPagination(data) {
    const container = document.getElementById('pagination');
    container.innerHTML = `
        <button ${!data.hasPrevious ? 'disabled' : ''} onclick="goToPage(${data.page - 1})">Anterior</button>
        <span class="page-info">Página ${data.page} de ${data.totalPages} (${data.totalCount} itens)</span>
        <button ${!data.hasNext ? 'disabled' : ''} onclick="goToPage(${data.page + 1})">Próxima</button>
    `;
}

function goToPage(page) {
    currentPage = page;
    loadProducts();
}

let deleteId = null;

function openDeleteModal(id, name) {
    deleteId = id;
    document.getElementById('delete-product-name').textContent = name;
    document.getElementById('delete-modal').style.display = 'flex';
    document.getElementById('confirm-delete-btn').onclick = confirmDelete;
}

function closeDeleteModal() {
    document.getElementById('delete-modal').style.display = 'none';
    deleteId = null;
}

async function confirmDelete() {
    if (!deleteId) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/produtos/${deleteId}`, { method: 'DELETE' });
        if (!res.ok) throw new Error('Erro ao excluir produto');

        closeDeleteModal();
        showAlert('Produto excluído com sucesso!', 'success');
        loadProducts();
    } catch (err) {
        closeDeleteModal();
        showAlert('Erro ao excluir produto: ' + err.message, 'error');
    }
}

function showAlert(message, type) {
    const container = document.getElementById('alert-container');
    container.innerHTML = `<div class="alert alert-${type}">${message}</div>`;
    setTimeout(() => container.innerHTML = '', 5000);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
