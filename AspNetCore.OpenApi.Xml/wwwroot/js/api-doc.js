let typeModal;
let currentLang = 'zh-CN';

// Localization strings
const i18n = {
    'zh-CN': {
        'version': '版本',
        'apiDoc': 'API 文档',
        'selectEndpoint': '请从左侧选择一个接口以查看详细信息',
        'routeParams': '路由参数',
        'queryParams': '查询参数',
        'headers': '请求头',
        'requestBody': '请求体',
        'responses': '响应',
        'noParams': '无参数',
        'paramName': '参数名',
        'type': '类型',
        'required': '必填',
        'optional': '可选',
        'description': '说明',
        'noResponse': '无响应体',
        'typeInfo': '类型信息',
        'namespace': '命名空间',
        'nullable': '可空',
        'yes': '是',
        'enumValues': '枚举值',
        'fields': '字段',
        'elementType': '元素类型',
        'dictionaryType': '字典类型',
        'keyType': '键类型',
        'valueType': '值类型',
        'tupleType': '元组类型',
        'tupleElements': '元组元素',
        'genericType': '泛型类型',
        'genericParams': '泛型参数',
        'name': '名称',
        'arrayType': '数组类型',
        'modelNotFound': '模型未找到'
    },
    'en-US': {
        'version': 'Version',
        'apiDoc': 'API Documentation',
        'selectEndpoint': 'Please select an endpoint from the left to view details',
        'routeParams': 'Route Parameters',
        'queryParams': 'Query Parameters',
        'headers': 'Headers',
        'requestBody': 'Request Body',
        'responses': 'Responses',
        'noParams': 'No parameters',
        'paramName': 'Parameter',
        'type': 'Type',
        'required': 'Required',
        'optional': 'Optional',
        'description': 'Description',
        'noResponse': 'No response body',
        'typeInfo': 'Type Information',
        'namespace': 'Namespace',
        'nullable': 'Nullable',
        'yes': 'Yes',
        'enumValues': 'Enum Values',
        'fields': 'Fields',
        'elementType': 'Element Type',
        'dictionaryType': 'Dictionary Type',
        'keyType': 'Key Type',
        'valueType': 'Value Type',
        'tupleType': 'Tuple Type',
        'tupleElements': 'Tuple Elements',
        'genericType': 'Generic Type',
        'genericParams': 'Generic Parameters',
        'name': 'Name',
        'arrayType': 'Array Type',
        'modelNotFound': 'Model not found'
    }
};

function t(key) {
    return i18n[currentLang]?.[key] || i18n['en-US']?.[key] || key;
}

function updateLocalization() {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });
    
    // Update empty state text
    const emptyState = document.getElementById('empty-state');
    if (emptyState) {
        emptyState.querySelector('h2').textContent = t('apiDoc');
        emptyState.querySelector('p').textContent = t('selectEndpoint');
    }
}

document.addEventListener('DOMContentLoaded', function() {
    typeModal = new bootstrap.Modal(document.getElementById('typeModal'));
    
    // Initialize theme from localStorage or system preference
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const theme = savedTheme || (prefersDark ? 'dark' : 'light');
    setTheme(theme);

    // Initialize language from localStorage or browser preference
    const savedLang = localStorage.getItem('lang');
    const browserLang = navigator.language;
    currentLang = savedLang || (browserLang.startsWith('zh') ? 'zh-CN' : 'en-US');
    document.getElementById('langToggle').textContent = currentLang === 'zh-CN' ? 'EN' : '中文';
    
    renderApiList();
    updateLocalization();

    // Theme toggle
    document.getElementById('themeToggle').addEventListener('click', function() {
        const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
    });

    // Language toggle
    document.getElementById('langToggle').addEventListener('click', function() {
        currentLang = currentLang === 'zh-CN' ? 'en-US' : 'zh-CN';
        localStorage.setItem('lang', currentLang);
        this.textContent = currentLang === 'zh-CN' ? 'EN' : '中文';
        updateLocalization();
        // Re-render the current endpoint if one is selected
        const activeEndpoint = document.querySelector('.endpoint-item.active');
        if (activeEndpoint) {
            const endpointIndex = Array.from(document.querySelectorAll('.endpoint-item')).indexOf(activeEndpoint);
            const allEndpoints = apiData.endpoints;
            if (allEndpoints[endpointIndex]) {
                showEndpoint(allEndpoints[endpointIndex]);
            }
        }
    });
});

function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    const icon = document.querySelector('#themeToggle i');
    if (theme === 'dark') {
        icon.className = 'bi bi-sun-fill';
    } else {
        icon.className = 'bi bi-moon-fill';
    }
}

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

        // Get unique action names from endpoints
        const actions = groupedEndpoints[controller].map(e => e.description).filter(Boolean);
        const uniqueActions = [...new Set(actions)];
        const actionBadges = uniqueActions.length > 0 && uniqueActions.length <= 10
            ? uniqueActions.map(action => `<span class="action-badge">${action}</span>`).join(' ')
            : '';

        const headerDiv = document.createElement('div');
        headerDiv.className = 'controller-header';
        headerDiv.innerHTML = `
            <span class="controller-name">
                ${controller}
                ${actionBadges}
            </span>
            <i class="bi bi-chevron-down controller-chevron"></i>
        `;

        const collapse = document.createElement('div');
        collapse.className = 'collapse show endpoint-list';
        collapse.id = `collapse-${controller.replace(/[^a-zA-Z0-9]/g, '-')}`;

        groupedEndpoints[controller].forEach(endpoint => {
            const item = document.createElement('div');
            item.className = 'endpoint-item';
            // Prioritize summary (XML comment), then path
            const displayText = endpoint.summary || endpoint.path;
            item.innerHTML = `
                <span class="badge method-badge method-${endpoint.method.toLowerCase()}">${endpoint.method}</span>
                <span class="endpoint-path">${displayText}</span>
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

    // Use summary (XML comment) if available, otherwise use path
    const title = endpoint.summary || endpoint.path;
    let html = `
        <div class="endpoint-detail-header">
            <h2 class="endpoint-title">${title}</h2>
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
                    <div class="card-header"><i class="bi bi-signpost"></i> ${t('routeParams')}</div>
                    <div class="card-body p-0">
                        ${renderParamsTable(req.routeParameters)}
                    </div>
                </div>
            `;
        }

        if (req.queryParameters && req.queryParameters.length > 0) {
            html += `
                <div class="card mb-3">
                    <div class="card-header"><i class="bi bi-question-circle"></i> ${t('queryParams')}</div>
                    <div class="card-body p-0">
                        ${renderParamsTable(req.queryParameters)}
                    </div>
                </div>
            `;
        }

        if (req.headers && req.headers.length > 0) {
            html += `
                <div class="card mb-3">
                    <div class="card-header"><i class="bi bi-list-ul"></i> ${t('headers')}</div>
                    <div class="card-body p-0">
                        ${renderParamsTable(req.headers)}
                    </div>
                </div>
            `;
        }

        if (req.body) {
            html += `
                <div class="card mb-3">
                    <div class="card-header"><i class="bi bi-file-earmark-code"></i> ${t('requestBody')}</div>
                    <div class="card-body p-0">
                        ${renderModel(req.body)}
                    </div>
                </div>
            `;
        }
    }

    if (endpoint.responses && endpoint.responses.length > 0) {
        html += `<div class="card mb-3">
            <div class="card-header"><i class="bi bi-reply"></i> ${t('responses')}</div>
            <div class="card-body">`;
        endpoint.responses.forEach(resp => {
            const statusClass = resp.statusCode >= 200 && resp.statusCode < 300 ? 'status-2xx' :
                               resp.statusCode >= 400 && resp.statusCode < 500 ? 'status-4xx' : 'status-5xx';
            // Default to application/json if content type is text/plain and there's a body
            let contentType = resp.contentType;
            if (contentType === 'text/plain' && resp.body) {
                contentType = 'application/json';
            }
            html += `
                <div class="mb-3">
                    <div class="mb-2">
                        <span class="status-badge ${statusClass}">${resp.statusCode}</span>
                        ${contentType ? `<code class="ms-2">${contentType}</code>` : ''}
                    </div>
                    ${resp.body ? renderModel(resp.body) : `<p class="text-muted mb-0">${t('noResponse')}</p>`}
                </div>
            `;
        });
        html += `</div></div>`;
    }

    detail.innerHTML = html;
}

function renderParamsTable(params) {
    if (!params || params.length === 0) {
        return `<p class="text-muted p-3 mb-0">${t('noParams')}</p>`;
    }

    let html = `
        <table class="table table-sm table-hover mb-0">
            <thead class="table-light">
                <tr>
                    <th style="width: 25%">${t('paramName')}</th>
                    <th style="width: 20%">${t('type')}</th>
                    <th style="width: 15%">${t('required')}</th>
                    <th style="width: 40%">${t('description')}</th>
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
                <td>${param.required ? `<span class="badge bg-danger">${t('required')}</span>` : `<span class="badge bg-secondary">${t('optional')}</span>`}</td>
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
    if (!model) return `<p class="text-muted mb-0">${t('noResponse')}</p>`;

    let html = '';
    
    if (model.fields && model.fields.length > 0) {
        html += renderParamsTable(model.fields);
    } else if (model.genericArguments && model.genericArguments.length > 0) {
        // For generic types with arguments, show type name with parameters
        const genericParams = model.genericArguments.map(arg => arg.name || arg.id || 'unknown').join(', ');
        
        html += `<div class="p-3"><p class="mb-2"><strong>${t('type')}: ${model.name || model.id} &lt; ${genericParams} &gt;</strong></p></div>`;
        
        // Show fields of each generic argument if available
        model.genericArguments.forEach((arg, idx) => {
            if (arg.fields && arg.fields.length > 0) {
                html += `<div class="p-3 pt-0"><p class="mb-2"><strong>Type Parameter ${idx + 1}: ${arg.name || arg.id}</strong></p></div>`;
                html += renderParamsTable(arg.fields);
            }
        });
    } else if (model.elementType) {
        // Array/List types
        if (model.elementType.fields && model.elementType.fields.length > 0) {
            html += `<div class="p-3"><p class="mb-2"><strong>${t('arrayType')}: ${model.name || model.id} &lt; ${model.elementType.name || model.elementType.id} &gt;</strong></p></div>`;
            html += renderParamsTable(model.elementType.fields);
        } else {
            html += `<p class="p-3 mb-0">${t('arrayType')}，${t('elementType')}: ${renderModelType(model.elementType)}</p>`;
        }
    } else if (model.keyType && model.valueType) {
        // Dictionary type
        html += `<div class="p-3">
            <p class="mb-2"><strong>${t('dictionaryType')}: ${model.name || model.id} &lt; ${model.keyType.name || model.keyType.id}, ${model.valueType.name || model.valueType.id} &gt;</strong></p>
            <p class="mb-1">${t('keyType')}: ${renderModelType(model.keyType)}</p>
            <p class="mb-0">${t('valueType')}: ${renderModelType(model.valueType)}</p>
        </div>`;
        if (model.valueType.fields && model.valueType.fields.length > 0) {
            html += renderParamsTable(model.valueType.fields);
        }
    } else if (model.tupleElements && model.tupleElements.length > 0) {
        html += `<div class="p-3"><p class="mb-2"><strong>${t('tupleType')}: ${model.name || model.id}</strong></p></div>`;
        // Show tuple elements as table
        const tupleFields = model.tupleElements.map((elem, idx) => ({
            name: elem.name || `Item${idx + 1}`,
            type: elem.type,
            required: elem.required,
            description: elem.description,
            modelId: elem.modelId
        }));
        html += renderParamsTable(tupleFields);
    } else if (model.modelType === 'Enum') {
        html += `<p class="p-3 mb-0">${t('type')}: ${model.name || model.id}</p>`;
    } else {
        html += `<p class="p-3 mb-0">${t('type')}: ${model.name || model.id || 'unknown'}</p>`;
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
        alert(t('modelNotFound') + ': ' + modelId);
        return;
    }

    document.getElementById('typeModalLabel').textContent = model.name || model.id;
    
    let html = `
        <div class="mb-3">
            ${model.namespace ? `<p class="mb-1"><strong>${t('namespace')}:</strong> ${model.namespace}</p>` : ''}
            <p class="mb-1"><strong>${t('type')}:</strong> ${model.modelType}</p>
            ${model.nullable ? `<p class="mb-1"><strong>${t('nullable')}:</strong> ${t('yes')}</p>` : ''}
            ${model.description ? `<p class="mb-1"><strong>${t('description')}:</strong> ${model.description}</p>` : ''}
        </div>
    `;

    if (model.modelType === 'Enum' && model.enumMembers && model.enumMembers.length > 0) {
        html += `<h6 class="mb-3">${t('enumValues')}</h6>`;
        model.enumMembers.forEach(member => {
            html += `
                <div class="enum-member">
                    <div class="enum-member-name">${member.name} = ${member.value}</div>
                    ${member.description ? `<div class="enum-member-value">${member.description}</div>` : ''}
                </div>
            `;
        });
    } else if (model.fields && model.fields.length > 0) {
        html += `<h6 class="mb-3">${t('fields')}</h6>${renderParamsTable(model.fields)}`;
    } else if (model.elementType) {
        html += `<h6 class="mb-2">${t('elementType')}</h6><p>${renderModelType(model.elementType)}</p>`;
    } else if (model.keyType && model.valueType) {
        html += `
            <h6 class="mb-2">${t('dictionaryType')}</h6>
            <p><strong>${t('keyType')}:</strong> ${renderModelType(model.keyType)}</p>
            <p><strong>${t('valueType')}:</strong> ${renderModelType(model.valueType)}</p>
        `;
    } else if (model.tupleElements && model.tupleElements.length > 0) {
        html += `<h6 class="mb-3">${t('tupleElements')}</h6>`;
        html += `
            <table class="table table-sm">
                <thead class="table-light">
                    <tr>
                        <th>${t('name')}</th>
                        <th>${t('type')}</th>
                    </tr>
                </thead>
                <tbody>
        `;
        model.tupleElements.forEach(elem => {
            const typeDisplay = elem.modelId 
                ? `<span class="type-link" onclick="showTypeModal('${elem.modelId}')">${elem.type || elem.modelId}</span>`
                : (elem.type || 'unknown');
            html += `
                <tr>
                    <td><code>${elem.name}</code></td>
                    <td>${typeDisplay}</td>
                </tr>
            `;
        });
        html += `</tbody></table>`;
    } else if (model.genericArguments && model.genericArguments.length > 0) {
        html += `<h6 class="mb-3">${t('genericParams')}</h6>`;
        html += `<ul class="list-group">`;
        model.genericArguments.forEach((arg, idx) => {
            html += `<li class="list-group-item">${idx + 1}. ${renderModelType(arg)}</li>`;
        });
        html += `</ul>`;
    }

    document.getElementById('typeModalBody').innerHTML = html;
    typeModal.show();
}
