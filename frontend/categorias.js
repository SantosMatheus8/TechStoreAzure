let currentPage = 1;
const pageSize = 10;
let editId = null;
let deleteId = null;

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
});

document.getElementById('category-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    await saveCategory();
});

async function loadCategories() {
    const loading = document.getElementById('loading');
    const table = document.getElementById('categories-table');
    const empty = document.getElementById('empty-state');

    loading.style.display = 'block';
    table.style.display = 'none';
    empty.style.display = 'none';

    try {
        const res = await fetch(`${API_BASE_URL}/api/categorias?page=${currentPage}&pageSize=${pageSize}`);
        if (!res.ok) throw new Error('Erro ao carregar categorias');
        const data = await res.json();

        loading.style.display = 'none';

        if (data.items.length === 0) {
            empty.style.display = 'block';
            return;
        }

        table.style.display = 'block';
        renderCategories(data.items);
        renderPagination(data);
    } catch (err) {
        loading.style.display = 'none';
        showAlert('Falha ao conectar com a API. Verifique se o servidor está rodando.', 'error');
    }
}

function renderCategories(categories) {
    const tbody = document.getElementById('categories-body');
    tbody.innerHTML = categories.map(c => `
        <tr>
            <td><strong>${escapeHtml(c.nome)}</strong></td>
            <td>${escapeHtml(c.descricao || '-')}</td>
            <td>
                <span class="badge ${c.ativo ? 'badge-active' : 'badge-inactive'}">
                    ${c.ativo ? 'Ativa' : 'Inativa'}
                </span>
            </td>
            <td>
                <button class="btn btn-outline btn-sm" onclick="startEdit('${c.id}', '${escapeJs(c.nome)}', '${escapeJs(c.descricao || '')}')">Editar</button>
                <button class="btn btn-danger btn-sm" onclick="openDeleteModal('${c.id}', '${escapeJs(c.nome)}')">Excluir</button>
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
    loadCategories();
}

async function saveCategory() {
    const body = {
        nome: document.getElementById('cat-nome').value.trim(),
        descricao: document.getElementById('cat-descricao').value.trim()
    };

    const btn = document.getElementById('submit-btn');
    btn.disabled = true;
    btn.textContent = 'Salvando...';

    try {
        const url = editId
            ? `${API_BASE_URL}/api/categorias/${editId}`
            : `${API_BASE_URL}/api/categorias`;

        const res = await fetch(url, {
            method: editId ? 'PUT' : 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            const err = await res.json();
            const msgs = err.errors
                ? Object.values(err.errors).flat().join(', ')
                : err.detail || 'Erro desconhecido';
            throw new Error(msgs);
        }

        showAlert(editId ? 'Categoria atualizada!' : 'Categoria criada!', 'success');
        cancelEdit();
        loadCategories();
    } catch (err) {
        showAlert(err.message, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Salvar';
    }
}

function startEdit(id, nome, descricao) {
    editId = id;
    document.getElementById('form-title').textContent = 'Editar Categoria';
    document.getElementById('cat-nome').value = nome;
    document.getElementById('cat-descricao').value = descricao;
    document.getElementById('cancel-btn').style.display = 'inline-flex';
    document.getElementById('cat-nome').focus();
}

function cancelEdit() {
    editId = null;
    document.getElementById('form-title').textContent = 'Nova Categoria';
    document.getElementById('cat-nome').value = '';
    document.getElementById('cat-descricao').value = '';
    document.getElementById('cancel-btn').style.display = 'none';
}

function openDeleteModal(id, name) {
    deleteId = id;
    document.getElementById('delete-cat-name').textContent = name;
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
        const res = await fetch(`${API_BASE_URL}/api/categorias/${deleteId}`, { method: 'DELETE' });
        if (!res.ok) throw new Error('Erro ao excluir categoria');

        closeDeleteModal();
        showAlert('Categoria excluída com sucesso!', 'success');
        loadCategories();
    } catch (err) {
        closeDeleteModal();
        showAlert('Erro ao excluir: ' + err.message, 'error');
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

function escapeJs(text) {
    return text.replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}
