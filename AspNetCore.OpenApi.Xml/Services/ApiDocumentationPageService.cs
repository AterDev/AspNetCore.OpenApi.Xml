using System.Text;
using System.Text.Json;
using OpenApi.Xml.Core.Models;

namespace AspNetCore.OpenApi.Xml.Services;

public interface IApiDocumentationPageService
{
    string GenerateHtml(ApiDocument document);
}

public class ApiDocumentationPageService : IApiDocumentationPageService
{
    public string GenerateHtml(ApiDocument document)
    {
        var jsonData = JsonSerializer.Serialize(document, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return $$"""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>{{document.Title}} - API Documentation</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: #f8f9fa;
        }

        .app-container {
            display: flex;
            height: 100vh;
        }

        .sidebar {
            width: 320px;
            background: #fff;
            border-right: 1px solid #dee2e6;
            overflow-y: auto;
            flex-shrink: 0;
        }

        .sidebar-header {
            padding: 1.5rem 1.25rem;
            border-bottom: 2px solid #0d6efd;
            background: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
            color: white;
        }

        .sidebar-header h1 {
            font-size: 1.25rem;
            margin: 0;
            font-weight: 600;
        }

        .sidebar-version {
            font-size: 0.875rem;
            opacity: 0.9;
            margin-top: 0.25rem;
        }

        .main-content {
            flex: 1;
            overflow-y: auto;
            padding: 2rem;
        }

        .controller-group {
            margin-bottom: 0.5rem;
        }

        .controller-header {
            background: #f8f9fa;
            padding: 0.75rem 1.25rem;
            cursor: pointer;
            border-left: 3px solid #6c757d;
            transition: all 0.2s;
            display: flex;
            justify-content: space-between;
            align-items: center;
            user-select: none;
        }

        .controller-header:hover {
            background: #e9ecef;
            border-left-color: #0d6efd;
        }

        .controller-header.active {
            border-left-color: #0d6efd;
            background: #e7f1ff;
        }

        .controller-name {
            font-weight: 600;
            font-size: 0.95rem;
            color: #212529;
        }

        .controller-chevron {
            transition: transform 0.2s;
            color: #6c757d;
        }

        .controller-header.collapsed .controller-chevron {
            transform: rotate(-90deg);
        }

        .endpoint-list {
            background: #fff;
        }

        .endpoint-item {
            padding: 0.625rem 1.25rem 0.625rem 2.5rem;
            cursor: pointer;
            border-left: 3px solid transparent;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .endpoint-item:hover {
            background: #f8f9fa;
            border-left-color: #0d6efd;
        }

        .endpoint-item.active {
            background: #e7f1ff;
            border-left-color: #0d6efd;
        }

        .method-badge {
            font-size: 0.7rem;
            font-weight: 700;
            padding: 0.25rem 0.5rem;
            min-width: 60px;
            text-align: center;
            border-radius: 0.25rem;
        }

        .method-get { background-color: #28a745; color: white; }
        .method-post { background-color: #0d6efd; color: white; }
        .method-put { background-color: #fd7e14; color: white; }
        .method-delete { background-color: #dc3545; color: white; }
        .method-patch { background-color: #6f42c1; color: white; }

        .endpoint-path {
            flex: 1;
            font-size: 0.875rem;
            color: #495057;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            color: #6c757d;
        }

        .empty-state i {
            font-size: 4rem;
            margin-bottom: 1rem;
            opacity: 0.3;
        }

        .endpoint-detail-header {
            margin-bottom: 1.5rem;
        }

        .endpoint-title {
            font-size: 1.75rem;
            font-weight: 600;
            margin-bottom: 1rem;
        }

        .endpoint-meta {
            display: flex;
            gap: 0.75rem;
            flex-wrap: wrap;
            align-items: center;
        }

        .type-link {
            color: #0d6efd;
            cursor: pointer;
            text-decoration: none;
            border-bottom: 1px dashed #0d6efd;
        }

        .type-link:hover {
            color: #0a58ca;
            border-bottom-style: solid;
        }

        .status-badge {
            font-weight: 600;
            padding: 0.35rem 0.75rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
        }

        .status-2xx { background-color: #d1e7dd; color: #0f5132; }
        .status-4xx { background-color: #fff3cd; color: #664d03; }
        .status-5xx { background-color: #f8d7da; color: #842029; }

        .enum-member {
            background: #f8f9fa;
            border-left: 3px solid #0d6efd;
            padding: 0.75rem;
            margin-bottom: 0.5rem;
            border-radius: 0.25rem;
        }

        .enum-member-name {
            font-weight: 600;
            font-size: 0.9rem;
            color: #212529;
        }

        .enum-member-value {
            color: #6c757d;
            font-size: 0.85rem;
            margin-top: 0.25rem;
        }

        .validation-text {
            font-size: 0.85rem;
            color: #6c757d;
            margin-top: 0.25rem;
        }

        code {
            background: #f8f9fa;
            padding: 0.125rem 0.375rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            color: #e83e8c;
        }

        .modal-body table {
            margin-bottom: 0;
        }
    </style>
</head>
<body>
    <div class="app-container">
        <aside class="sidebar">
            <div class="sidebar-header">
                <h1>{{document.Title}}</h1>
                <div class="sidebar-version">
                    <i class="bi bi-tag"></i> Version: {{document.Version}}
                </div>
            </div>
            <div id="api-list" class="p-0"></div>
        </aside>
        <main class="main-content">
            <div class="empty-state" id="empty-state">
                <i class="bi bi-file-earmark-text"></i>
                <h2>API 文档</h2>
                <p class="text-muted">请从左侧选择一个接口以查看详细信息</p>
            </div>
            <div id="endpoint-detail" style="display: none;"></div>
        </main>
    </div>

    <!-- Type Modal -->
    <div class="modal fade" id="typeModal" tabindex="-1" aria-labelledby="typeModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="typeModalLabel">Type Information</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body" id="typeModalBody"></div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous"></script>
    <script>
        const apiData = {{jsonData}};
        let typeModal;

        document.addEventListener('DOMContentLoaded', function() {
            typeModal = new bootstrap.Modal(document.getElementById('typeModal'));
            renderApiList();
        });

        function renderApiList() {
            const groupedEndpoints = {};
            apiData.endpoints.forEach(endpoint => {
                const tag = endpoint.tags && endpoint.tags.length > 0 ? endpoint.tags[0] : 'Default';
                if (!groupedEndpoints[tag]) {
                    groupedEndpoints[tag] = [];
                }
                groupedEndpoints[tag].push(endpoint);
            });

            const apiList = document.getElementById('api-list');
            Object.keys(groupedEndpoints).sort().forEach(controller => {
                const groupDiv = document.createElement('div');
                groupDiv.className = 'controller-group';

                const headerDiv = document.createElement('div');
                headerDiv.className = 'controller-header';
                headerDiv.innerHTML = `
                    <span class="controller-name">${controller}</span>
                    <i class="bi bi-chevron-down controller-chevron"></i>
                `;

                const collapse = document.createElement('div');
                collapse.className = 'collapse show endpoint-list';
                collapse.id = `collapse-${controller.replace(/[^a-zA-Z0-9]/g, '-')}`;

                groupedEndpoints[controller].forEach(endpoint => {
                    const item = document.createElement('div');
                    item.className = 'endpoint-item';
                    item.innerHTML = `
                        <span class="badge method-badge method-${endpoint.method.toLowerCase()}">${endpoint.method}</span>
                        <span class="endpoint-path">${endpoint.path}</span>
                    `;
                    item.onclick = () => {
                        document.querySelectorAll('.endpoint-item').forEach(i => i.classList.remove('active'));
                        item.classList.add('active');
                        showEndpoint(endpoint);
                    };
                    collapse.appendChild(item);
                });

                headerDiv.onclick = () => {
                    headerDiv.classList.toggle('collapsed');
                    const bsCollapse = new bootstrap.Collapse(collapse, {toggle: true});
                };

                groupDiv.appendChild(headerDiv);
                groupDiv.appendChild(collapse);
                apiList.appendChild(groupDiv);
            });
        }

        function showEndpoint(endpoint) {
            document.getElementById('empty-state').style.display = 'none';
            const detail = document.getElementById('endpoint-detail');
            detail.style.display = 'block';

            let html = `
                <div class="endpoint-detail-header">
                    <h2 class="endpoint-title">${endpoint.summary || endpoint.path}</h2>
                    <div class="endpoint-meta">
                        <span class="badge method-badge method-${endpoint.method.toLowerCase()}">${endpoint.method}</span>
                        <code>${endpoint.path}</code>
                        ${endpoint.deprecated ? '<span class="badge bg-danger">DEPRECATED</span>' : ''}
                        ${endpoint.tags ? endpoint.tags.map(t => `<span class="badge bg-secondary">${t}</span>`).join('') : ''}
                    </div>
                </div>
            `;

            if (endpoint.description) {
                html += `<div class="card mb-3"><div class="card-body">${endpoint.description}</div></div>`;
            }

            if (endpoint.request) {
                const req = endpoint.request;
                
                if (req.routeParameters && req.routeParameters.length > 0) {
                    html += `
                        <div class="card mb-3">
                            <div class="card-header"><i class="bi bi-signpost"></i> 路由参数 (Route Parameters)</div>
                            <div class="card-body p-0">
                                ${renderParamsTable(req.routeParameters)}
                            </div>
                        </div>
                    `;
                }

                if (req.queryParameters && req.queryParameters.length > 0) {
                    html += `
                        <div class="card mb-3">
                            <div class="card-header"><i class="bi bi-question-circle"></i> 查询参数 (Query Parameters)</div>
                            <div class="card-body p-0">
                                ${renderParamsTable(req.queryParameters)}
                            </div>
                        </div>
                    `;
                }

                if (req.headers && req.headers.length > 0) {
                    html += `
                        <div class="card mb-3">
                            <div class="card-header"><i class="bi bi-list-ul"></i> 请求头 (Headers)</div>
                            <div class="card-body p-0">
                                ${renderParamsTable(req.headers)}
                            </div>
                        </div>
                    `;
                }

                if (req.body) {
                    html += `
                        <div class="card mb-3">
                            <div class="card-header"><i class="bi bi-file-earmark-code"></i> 请求体 (Request Body)</div>
                            <div class="card-body p-0">
                                ${renderModel(req.body)}
                            </div>
                        </div>
                    `;
                }
            }

            if (endpoint.responses && endpoint.responses.length > 0) {
                html += `<div class="card mb-3">
                    <div class="card-header"><i class="bi bi-reply"></i> 响应 (Responses)</div>
                    <div class="card-body">`;
                endpoint.responses.forEach(resp => {
                    const statusClass = resp.statusCode >= 200 && resp.statusCode < 300 ? 'status-2xx' :
                                       resp.statusCode >= 400 && resp.statusCode < 500 ? 'status-4xx' : 'status-5xx';
                    html += `
                        <div class="mb-3">
                            <div class="mb-2">
                                <span class="status-badge ${statusClass}">${resp.statusCode}</span>
                                ${resp.contentType ? `<code class="ms-2">${resp.contentType}</code>` : ''}
                            </div>
                            ${resp.body ? renderModel(resp.body) : '<p class="text-muted mb-0">无响应体</p>'}
                        </div>
                    `;
                });
                html += `</div></div>`;
            }

            detail.innerHTML = html;
        }

        function renderParamsTable(params) {
            if (!params || params.length === 0) {
                return '<p class="text-muted p-3 mb-0">无参数</p>';
            }

            let html = `
                <table class="table table-sm table-hover mb-0">
                    <thead class="table-light">
                        <tr>
                            <th style="width: 25%">参数名</th>
                            <th style="width: 20%">类型</th>
                            <th style="width: 15%">必填</th>
                            <th style="width: 40%">说明</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            params.forEach(param => {
                const typeDisplay = param.modelId 
                    ? `<span class="type-link" onclick="showTypeModal('${param.modelId}')">${param.type || param.modelId}</span>`
                    : (param.type || 'any');
                
                html += `
                    <tr>
                        <td><code>${param.name}</code></td>
                        <td>${typeDisplay}</td>
                        <td>${param.required ? '<span class="badge bg-danger">必填</span>' : '<span class="badge bg-secondary">可选</span>'}</td>
                        <td>
                            ${param.description || '-'}
                            ${renderValidation(param)}
                        </td>
                    </tr>
                `;
            });

            html += `</tbody></table>`;
            return html;
        }

        function renderValidation(field) {
            const validations = [];
            if (field.minLength) validations.push(`最小长度: ${field.minLength}`);
            if (field.maxLength) validations.push(`最大长度: ${field.maxLength}`);
            if (field.minimum) validations.push(`最小值: ${field.minimum}`);
            if (field.maximum) validations.push(`最大值: ${field.maximum}`);
            if (field.pattern) validations.push(`模式: ${field.pattern}`);
            return validations.length > 0 ? `<div class="validation-text">${validations.join(', ')}</div>` : '';
        }

        function renderModel(model) {
            if (!model) return '<p class="text-muted mb-0">无模型信息</p>';

            let html = '';
            
            if (model.fields && model.fields.length > 0) {
                html += renderParamsTable(model.fields);
            } else if (model.elementType) {
                html += `<p class="p-3 mb-0">数组类型: ${renderModelType(model.elementType)}</p>`;
            } else if (model.modelType === 'Enum') {
                html += `<p class="p-3 mb-0">枚举类型</p>`;
            } else {
                html += `<p class="p-3 mb-0">类型: ${model.name || model.id || 'unknown'}</p>`;
            }

            return html;
        }

        function renderModelType(model) {
            if (!model) return 'unknown';
            if (model.id) {
                return `<span class="type-link" onclick="showTypeModal('${model.id}')">${model.name || model.id}</span>`;
            }
            return model.name || model.id || 'unknown';
        }

        function showTypeModal(modelId) {
            const model = apiData.models.find(m => m.id === modelId);
            if (!model) {
                alert('模型未找到: ' + modelId);
                return;
            }

            document.getElementById('typeModalLabel').textContent = model.name || model.id;
            
            let html = `
                <div class="mb-3">
                    ${model.namespace ? `<p class="mb-1"><strong>命名空间:</strong> ${model.namespace}</p>` : ''}
                    <p class="mb-1"><strong>类型:</strong> ${model.modelType}</p>
                    ${model.nullable ? '<p class="mb-1"><strong>可空:</strong> 是</p>' : ''}
                    ${model.description ? `<p class="mb-1"><strong>说明:</strong> ${model.description}</p>` : ''}
                </div>
            `;

            if (model.modelType === 'Enum' && model.enumMembers && model.enumMembers.length > 0) {
                html += `<h6 class="mb-3">枚举值</h6>`;
                model.enumMembers.forEach(member => {
                    html += `
                        <div class="enum-member">
                            <div class="enum-member-name">${member.name} = ${member.value}</div>
                            ${member.description ? `<div class="enum-member-value">${member.description}</div>` : ''}
                        </div>
                    `;
                });
            } else if (model.fields && model.fields.length > 0) {
                html += `<h6 class="mb-3">字段</h6>${renderParamsTable(model.fields)}`;
            } else if (model.elementType) {
                html += `<h6 class="mb-2">元素类型</h6><p>${renderModelType(model.elementType)}</p>`;
            } else if (model.keyType && model.valueType) {
                html += `
                    <h6 class="mb-2">字典类型</h6>
                    <p><strong>键类型:</strong> ${renderModelType(model.keyType)}</p>
                    <p><strong>值类型:</strong> ${renderModelType(model.valueType)}</p>
                `;
            }

            document.getElementById('typeModalBody').innerHTML = html;
            typeModal.show();
        }
    </script>
</body>
</html>
""";
    }
}

            display: block;
        }

        .endpoint-list.collapsed {
            display: none;
        }

        .endpoint-item {
            padding: 8px 20px 8px 40px;
            cursor: pointer;
            font-size: 13px;
            border-left: 3px solid transparent;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .endpoint-item:hover {
            background: #f6f8fa;
        }

        .endpoint-item.active {
            background: #f1f8ff;
            border-left-color: #0366d6;
        }

        .method-badge {
            font-size: 10px;
            font-weight: 700;
            padding: 2px 6px;
            border-radius: 3px;
            color: white;
            min-width: 50px;
            text-align: center;
        }

        .method-get { background: #28a745; }
        .method-post { background: #0366d6; }
        .method-put { background: #ffa500; }
        .method-delete { background: #dc3545; }
        .method-patch { background: #6f42c1; }

        .endpoint-path {
            flex: 1;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .content {
            flex: 1;
            overflow-y: auto;
            padding: 30px 40px;
        }

        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #586069;
        }

        .empty-state h2 {
            font-size: 24px;
            margin-bottom: 10px;
        }

        .endpoint-detail h2 {
            font-size: 28px;
            margin-bottom: 10px;
            color: #24292e;
        }

        .endpoint-meta {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
            flex-wrap: wrap;
        }

        .meta-item {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }

        .deprecated-badge {
            background: #dc3545;
            color: white;
            padding: 2px 8px;
            border-radius: 3px;
            font-size: 12px;
        }

        .tag-badge {
            background: #e1e4e8;
            color: #24292e;
            padding: 2px 8px;
            border-radius: 3px;
            font-size: 12px;
        }

        .section {
            background: white;
            border: 1px solid #e1e4e8;
            border-radius: 6px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .section h3 {
            font-size: 18px;
            margin-bottom: 15px;
            color: #24292e;
            border-bottom: 1px solid #e1e4e8;
            padding-bottom: 10px;
        }

        .params-table {
            width: 100%;
            border-collapse: collapse;
        }

        .params-table th {
            background: #f6f8fa;
            padding: 10px;
            text-align: left;
            font-size: 13px;
            font-weight: 600;
            border-bottom: 1px solid #e1e4e8;
        }

        .params-table td {
            padding: 10px;
            font-size: 13px;
            border-bottom: 1px solid #e1e4e8;
        }

        .params-table tr:last-child td {
            border-bottom: none;
        }

        .required-badge {
            background: #dc3545;
            color: white;
            padding: 1px 5px;
            border-radius: 3px;
            font-size: 10px;
            font-weight: 600;
        }

        .type-link {
            color: #0366d6;
            cursor: pointer;
            text-decoration: underline;
        }

        .type-link:hover {
            color: #0256c7;
        }

        .no-data {
            color: #586069;
            font-style: italic;
            padding: 10px;
        }

        .modal {
            display: none;
            position: fixed;
            z-index: 1000;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
        }

        .modal.active {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .modal-content {
            background: white;
            border-radius: 6px;
            max-width: 800px;
            max-height: 80vh;
            width: 90%;
            overflow-y: auto;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
        }

        .modal-header {
            padding: 20px;
            border-bottom: 1px solid #e1e4e8;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .modal-header h3 {
            font-size: 20px;
            margin: 0;
        }

        .modal-close {
            background: none;
            border: none;
            font-size: 24px;
            cursor: pointer;
            color: #586069;
            padding: 0;
            width: 30px;
            height: 30px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 3px;
        }

        .modal-close:hover {
            background: #e1e4e8;
        }

        .modal-body {
            padding: 20px;
        }

        .model-info {
            margin-bottom: 15px;
        }

        .model-info-item {
            display: flex;
            gap: 10px;
            margin-bottom: 5px;
            font-size: 13px;
        }

        .model-info-label {
            font-weight: 600;
            min-width: 80px;
        }

        .enum-members {
            margin-top: 10px;
        }

        .enum-member {
            padding: 8px;
            background: #f6f8fa;
            border-left: 3px solid #0366d6;
            margin-bottom: 5px;
        }

        .enum-member-name {
            font-weight: 600;
            font-size: 13px;
        }

        .enum-member-value {
            color: #586069;
            font-size: 12px;
        }

        .response-status {
            display: inline-block;
            padding: 2px 8px;
            border-radius: 3px;
            font-size: 12px;
            font-weight: 600;
        }

        .status-2xx { background: #28a745; color: white; }
        .status-4xx { background: #ffa500; color: white; }
        .status-5xx { background: #dc3545; color: white; }

        code {
            background: #f6f8fa;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 12px;
            font-family: 'Consolas', 'Monaco', monospace;
        }
    </style>
</head>
<body>
    <div class="container">
        <aside class="sidebar">
            <h1>{{document.Title}}</h1>
            <div class="version">Version: {{document.Version}}</div>
            <div id="api-list"></div>
        </aside>
        <main class="content">
            <div class="empty-state" id="empty-state">
                <h2>API 文档</h2>
                <p>请从左侧选择一个接口以查看详细信息</p>
            </div>
            <div id="endpoint-detail" style="display: none;"></div>
        </main>
    </div>

    <div class="modal" id="type-modal">
        <div class="modal-content">
            <div class="modal-header">
                <h3 id="modal-title">Type Information</h3>
                <button class="modal-close" onclick="closeModal()">&times;</button>
            </div>
            <div class="modal-body" id="modal-body"></div>
        </div>
    </div>

    <script>
        const apiData = {{jsonData}};

        // Group endpoints by controller
        const groupedEndpoints = {};
        apiData.endpoints.forEach(endpoint => {
            const tag = endpoint.tags && endpoint.tags.length > 0 ? endpoint.tags[0] : 'Default';
            if (!groupedEndpoints[tag]) {
                groupedEndpoints[tag] = [];
            }
            groupedEndpoints[tag].push(endpoint);
        });

        // Render API list
        const apiList = document.getElementById('api-list');
        Object.keys(groupedEndpoints).sort().forEach(controller => {
            const group = document.createElement('div');
            group.className = 'controller-group';

            const title = document.createElement('div');
            title.className = 'controller-title';
            title.innerHTML = `
                <span>${controller}</span>
                <span class="arrow">▼</span>
            `;
            title.onclick = () => {
                title.classList.toggle('collapsed');
                endpointList.classList.toggle('collapsed');
            };

            const endpointList = document.createElement('div');
            endpointList.className = 'endpoint-list';

            groupedEndpoints[controller].forEach(endpoint => {
                const item = document.createElement('div');
                item.className = 'endpoint-item';
                item.innerHTML = `
                    <span class="method-badge method-${endpoint.method.toLowerCase()}">${endpoint.method}</span>
                    <span class="endpoint-path">${endpoint.path}</span>
                `;
                item.onclick = () => {
                    document.querySelectorAll('.endpoint-item').forEach(i => i.classList.remove('active'));
                    item.classList.add('active');
                    showEndpoint(endpoint);
                };
                endpointList.appendChild(item);
            });

            group.appendChild(title);
            group.appendChild(endpointList);
            apiList.appendChild(group);
        });

        // Show endpoint details
        function showEndpoint(endpoint) {
            document.getElementById('empty-state').style.display = 'none';
            const detail = document.getElementById('endpoint-detail');
            detail.style.display = 'block';

            let html = `
                <h2>${endpoint.summary || endpoint.path}</h2>
                <div class="endpoint-meta">
                    <div class="meta-item">
                        <span class="method-badge method-${endpoint.method.toLowerCase()}">${endpoint.method}</span>
                        <code>${endpoint.path}</code>
                    </div>
                    ${endpoint.deprecated ? '<span class="deprecated-badge">DEPRECATED</span>' : ''}
                    ${endpoint.tags ? endpoint.tags.map(t => `<span class="tag-badge">${t}</span>`).join('') : ''}
                </div>
            `;

            if (endpoint.description) {
                html += `<div class="section"><p>${endpoint.description}</p></div>`;
            }

            // Request section
            if (endpoint.request) {
                const req = endpoint.request;
                
                // Route Parameters
                if (req.routeParameters && req.routeParameters.length > 0) {
                    html += `
                        <div class="section">
                            <h3>路由参数 (Route Parameters)</h3>
                            ${renderParamsTable(req.routeParameters)}
                        </div>
                    `;
                }

                // Query Parameters
                if (req.queryParameters && req.queryParameters.length > 0) {
                    html += `
                        <div class="section">
                            <h3>查询参数 (Query Parameters)</h3>
                            ${renderParamsTable(req.queryParameters)}
                        </div>
                    `;
                }

                // Headers
                if (req.headers && req.headers.length > 0) {
                    html += `
                        <div class="section">
                            <h3>请求头 (Headers)</h3>
                            ${renderParamsTable(req.headers)}
                        </div>
                    `;
                }

                // Request Body
                if (req.body) {
                    html += `
                        <div class="section">
                            <h3>请求体 (Request Body)</h3>
                            ${renderModel(req.body)}
                        </div>
                    `;
                }
            }

            // Responses
            if (endpoint.responses && endpoint.responses.length > 0) {
                html += `<div class="section"><h3>响应 (Responses)</h3>`;
                endpoint.responses.forEach(resp => {
                    const statusClass = resp.statusCode >= 200 && resp.statusCode < 300 ? 'status-2xx' :
                                       resp.statusCode >= 400 && resp.statusCode < 500 ? 'status-4xx' : 'status-5xx';
                    html += `
                        <div style="margin-bottom: 20px;">
                            <div style="margin-bottom: 10px;">
                                <span class="response-status ${statusClass}">${resp.statusCode}</span>
                                ${resp.contentType ? `<code>${resp.contentType}</code>` : ''}
                            </div>
                            ${resp.body ? renderModel(resp.body) : '<div class="no-data">无响应体</div>'}
                        </div>
                    `;
                });
                html += `</div>`;
            }

            detail.innerHTML = html;
        }

        function renderParamsTable(params) {
            if (!params || params.length === 0) {
                return '<div class="no-data">无参数</div>';
            }

            let html = `
                <table class="params-table">
                    <thead>
                        <tr>
                            <th>参数名</th>
                            <th>类型</th>
                            <th>必填</th>
                            <th>说明</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            params.forEach(param => {
                const typeDisplay = param.modelId 
                    ? `<span class="type-link" onclick="showTypeModal('${param.modelId}')">${param.type || param.modelId}</span>`
                    : (param.type || 'any');
                
                html += `
                    <tr>
                        <td><code>${param.name}</code></td>
                        <td>${typeDisplay}</td>
                        <td>${param.required ? '<span class="required-badge">必填</span>' : '可选'}</td>
                        <td>${param.description || '-'}${renderValidation(param)}</td>
                    </tr>
                `;
            });

            html += `</tbody></table>`;
            return html;
        }

        function renderValidation(field) {
            const validations = [];
            if (field.minLength) validations.push(`最小长度: ${field.minLength}`);
            if (field.maxLength) validations.push(`最大长度: ${field.maxLength}`);
            if (field.minimum) validations.push(`最小值: ${field.minimum}`);
            if (field.maximum) validations.push(`最大值: ${field.maximum}`);
            if (field.pattern) validations.push(`模式: ${field.pattern}`);
            return validations.length > 0 ? `<br><small style="color: #586069;">${validations.join(', ')}</small>` : '';
        }

        function renderModel(model) {
            if (!model) return '<div class="no-data">无模型信息</div>';

            let html = '';
            
            if (model.fields && model.fields.length > 0) {
                html += renderParamsTable(model.fields);
            } else if (model.elementType) {
                html += `<div>数组类型: ${renderModelType(model.elementType)}</div>`;
            } else if (model.modelType === 'Enum') {
                html += `<div>枚举类型</div>`;
            } else {
                html += `<div>类型: ${model.name || model.id || 'unknown'}</div>`;
            }

            return html;
        }

        function renderModelType(model) {
            if (!model) return 'unknown';
            if (model.id) {
                return `<span class="type-link" onclick="showTypeModal('${model.id}')">${model.name || model.id}</span>`;
            }
            return model.name || model.id || 'unknown';
        }

        function showTypeModal(modelId) {
            const model = apiData.models.find(m => m.id === modelId);
            if (!model) {
                alert('模型未找到: ' + modelId);
                return;
            }

            document.getElementById('modal-title').textContent = model.name || model.id;
            
            let html = `
                <div class="model-info">
                    ${model.namespace ? `<div class="model-info-item"><span class="model-info-label">命名空间:</span><span>${model.namespace}</span></div>` : ''}
                    <div class="model-info-item"><span class="model-info-label">类型:</span><span>${model.modelType}</span></div>
                    ${model.nullable ? '<div class="model-info-item"><span class="model-info-label">可空:</span><span>是</span></div>' : ''}
                    ${model.description ? `<div class="model-info-item"><span class="model-info-label">说明:</span><span>${model.description}</span></div>` : ''}
                </div>
            `;

            if (model.modelType === 'Enum' && model.enumMembers && model.enumMembers.length > 0) {
                html += `<h4>枚举值</h4><div class="enum-members">`;
                model.enumMembers.forEach(member => {
                    html += `
                        <div class="enum-member">
                            <div class="enum-member-name">${member.name} = ${member.value}</div>
                            ${member.description ? `<div class="enum-member-value">${member.description}</div>` : ''}
                        </div>
                    `;
                });
                html += `</div>`;
            } else if (model.fields && model.fields.length > 0) {
                html += `<h4>字段</h4>${renderParamsTable(model.fields)}`;
            } else if (model.elementType) {
                html += `<h4>元素类型</h4><div>${renderModelType(model.elementType)}</div>`;
            } else if (model.keyType && model.valueType) {
                html += `<h4>字典类型</h4>`;
                html += `<div>键类型: ${renderModelType(model.keyType)}</div>`;
                html += `<div>值类型: ${renderModelType(model.valueType)}</div>`;
            }

            document.getElementById('modal-body').innerHTML = html;
            document.getElementById('type-modal').classList.add('active');
        }

        function closeModal() {
            document.getElementById('type-modal').classList.remove('active');
        }

        // Close modal on background click
        document.getElementById('type-modal').onclick = function(e) {
            if (e.target === this) {
                closeModal();
            }
        };
    </script>
</body>
</html>
""";
    }
}
