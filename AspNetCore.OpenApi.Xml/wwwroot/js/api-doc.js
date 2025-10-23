let typeModal;
let currentLang = 'zh-CN';

// Localization strings
const i18n = {
    'zh-CN': {
        'version': '版本',
        'apiDoc': 'API 文档',
        'selectEndpoint': '请从左侧选择一个接口以查看详细信息',
        'searchEndpoints': '搜索接口...',
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
        'modelNotFound': '模型未找到',
        'responseExample': '响应示例'
    },
    'en-US': {
        'version': 'Version',
        'apiDoc': 'API Documentation',
        'selectEndpoint': 'Please select an endpoint from the left to view details',
        'searchEndpoints': 'Search endpoints...',
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
        'modelNotFound': 'Model not found',
        'responseExample': 'Response Example'
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
    
    // Update placeholder attributes
    document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
        const key = el.getAttribute('data-i18n-placeholder');
        el.placeholder = t(key);
    });
    
    // Update empty state text
    const emptyState = document.getElementById('empty-state');
    if (emptyState) {
        emptyState.querySelector('h2').textContent = t('apiDoc');
        emptyState.querySelector('p').textContent = t('selectEndpoint');
    }
}

document.addEventListener('DOMContentLoaded', function() {
    // Initialize modal element reference (will work without Bootstrap Modal)
    const modalElement = document.getElementById('typeModal');
    typeModal = {
        show: function() {
            if (modalElement) {
                modalElement.style.display = 'block';
                modalElement.classList.add('show');
                modalElement.setAttribute('aria-modal', 'true');
                modalElement.removeAttribute('aria-hidden');
                // Add backdrop
                let backdrop = document.querySelector('.modal-backdrop');
                if (!backdrop) {
                    backdrop = document.createElement('div');
                    backdrop.className = 'modal-backdrop fade show';
                    document.body.appendChild(backdrop);
                }
                document.body.classList.add('modal-open');
            }
        },
        hide: function() {
            if (modalElement) {
                modalElement.style.display = 'none';
                modalElement.classList.remove('show');
                modalElement.setAttribute('aria-hidden', 'true');
                modalElement.removeAttribute('aria-modal');
                // Remove backdrop
                const backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) {
                    backdrop.remove();
                }
                document.body.classList.remove('modal-open');
            }
        }
    };
    
    // Setup close button for modal
    const closeButton = modalElement?.querySelector('.btn-close');
    if (closeButton) {
        closeButton.addEventListener('click', () => typeModal.hide());
    }
    
    // Close modal on backdrop click
    if (modalElement) {
        modalElement.addEventListener('click', (e) => {
            if (e.target === modalElement) {
                typeModal.hide();
            }
        });
    }
    
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
    
    // Initialize endpoint click handlers on pre-rendered elements
    initializeEndpointHandlers();
    updateLocalization();

    // Setup search functionality
    const searchInput = document.getElementById('endpoint-search');
    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            filterEndpoints(e.target.value);
        });
    }

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
            const endpointIndex = parseInt(activeEndpoint.getAttribute('data-endpoint-index'));
            if (!isNaN(endpointIndex) && apiData.endpoints[endpointIndex]) {
                showEndpoint(apiData.endpoints[endpointIndex]);
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

function initializeEndpointHandlers() {
    // Setup click handlers for controller headers (collapse/expand)
    document.querySelectorAll('.controller-header').forEach(header => {
        header.addEventListener('click', function() {
            this.classList.toggle('collapsed');
            const collapse = this.nextElementSibling;
            if (collapse && collapse.classList.contains('endpoint-list')) {
                if (collapse.style.display === 'none') {
                    collapse.style.display = 'block';
                } else {
                    collapse.style.display = 'none';
                }
            }
        });
    });

    // Setup click handlers for endpoint items
    document.querySelectorAll('.endpoint-item').forEach(item => {
        item.addEventListener('click', function() {
            document.querySelectorAll('.endpoint-item').forEach(i => i.classList.remove('active'));
            this.classList.add('active');
            const endpointIndex = parseInt(this.getAttribute('data-endpoint-index'));
            if (!isNaN(endpointIndex) && apiData.endpoints[endpointIndex]) {
                showEndpoint(apiData.endpoints[endpointIndex]);
            }
        });
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
                    ${resp.body ? renderJsonExample(resp.body) : ''}
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
        const keyTypeName = renderModelTypeSimple(model.keyType);
        const valueTypeName = renderModelTypeSimple(model.valueType);
        html += `<div class="p-3">
            <p class="mb-2"><strong>${t('dictionaryType')}: ${model.name || 'Dictionary'}&lt;${keyTypeName}, ${valueTypeName}&gt;</strong></p>
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
    
    // Build a friendly type name with generic parameters
    let typeName = model.name || model.id || 'unknown';
    
    // Handle Dictionary types
    if (model.keyType && model.valueType) {
        const keyTypeName = renderModelTypeSimple(model.keyType);
        const valueTypeName = renderModelTypeSimple(model.valueType);
        typeName = `${model.name || 'Dictionary'}&lt;${keyTypeName}, ${valueTypeName}&gt;`;
    }
    // Handle Array/List types
    else if (model.elementType) {
        const elemTypeName = renderModelTypeSimple(model.elementType);
        typeName = `${model.name || 'Array'}&lt;${elemTypeName}&gt;`;
    }
    // Handle generic types with generic arguments
    else if (model.genericArguments && model.genericArguments.length > 0) {
        const genericParams = model.genericArguments.map(arg => renderModelTypeSimple(arg)).join(', ');
        typeName = `${model.name || model.id}&lt;${genericParams}&gt;`;
    }
    
    if (model.id) {
        return `<span class="type-link" onclick="showTypeModal('${model.id}')">${typeName}</span>`;
    }
    return typeName;
}

function renderModelTypeSimple(model) {
    if (!model) return 'unknown';
    
    // Build a simple type name without links
    let typeName = model.name || model.id || 'unknown';
    
    // Handle Dictionary types
    if (model.keyType && model.valueType) {
        const keyTypeName = renderModelTypeSimple(model.keyType);
        const valueTypeName = renderModelTypeSimple(model.valueType);
        typeName = `${model.name || 'Dictionary'}<${keyTypeName}, ${valueTypeName}>`;
    }
    // Handle Array/List types
    else if (model.elementType) {
        const elemTypeName = renderModelTypeSimple(model.elementType);
        typeName = `${model.name || 'Array'}<${elemTypeName}>`;
    }
    // Handle generic types with generic arguments
    else if (model.genericArguments && model.genericArguments.length > 0) {
        const genericParams = model.genericArguments.map(arg => renderModelTypeSimple(arg)).join(', ');
        typeName = `${model.name || model.id}<${genericParams}>`;
    }
    
    return typeName;
}

function renderJsonExample(model) {
    if (!model) return '';
    
    const example = generateJsonExample(model);
    if (!example) return '';
    
    try {
        const jsonString = JSON.stringify(example, null, 2);
        return `
            <div class="mt-4">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <strong class="text-secondary">${t('responseExample')}</strong>
                    <button class="btn btn-sm btn-outline-secondary copy-json" onclick="copyJsonExample(this)" data-json="${escapeHtml(jsonString)}">
                        <i class="bi bi-clipboard"></i> Copy
                    </button>
                </div>
                <pre class="json-example"><code>${escapeHtml(jsonString)}</code></pre>
            </div>
        `;
    } catch (e) {
        console.error('Failed to generate JSON example:', e);
        return '';
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function copyJsonExample(button) {
    const json = button.getAttribute('data-json');
    navigator.clipboard.writeText(json).then(() => {
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="bi bi-check"></i> Copied!';
        button.classList.add('btn-success');
        button.classList.remove('btn-outline-secondary');
        setTimeout(() => {
            button.innerHTML = originalText;
            button.classList.remove('btn-success');
            button.classList.add('btn-outline-secondary');
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy:', err);
    });
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

function filterEndpoints(searchText) {
    const search = searchText.toLowerCase().trim();
    const controllerGroups = document.querySelectorAll('.controller-group');
    
    if (!search) {
        // Show all endpoints and groups
        controllerGroups.forEach(group => {
            group.style.display = '';
            const endpoints = group.querySelectorAll('.endpoint-item');
            endpoints.forEach(item => item.style.display = '');
        });
        return;
    }
    
    controllerGroups.forEach(group => {
        const endpoints = group.querySelectorAll('.endpoint-item');
        let hasVisibleEndpoint = false;
        
        endpoints.forEach(item => {
            const endpoint = item.textContent.toLowerCase();
            // Get the actual endpoint data using the data attribute
            const endpointIndex = parseInt(item.getAttribute('data-endpoint-index'));
            const endpointData = !isNaN(endpointIndex) ? apiData.endpoints[endpointIndex] : null;
            
            // Also search in data attributes for path, summary, and description
            const dataPath = item.getAttribute('data-endpoint-path')?.toLowerCase() || '';
            const dataSummary = item.getAttribute('data-endpoint-summary')?.toLowerCase() || '';
            const dataDescription = item.getAttribute('data-endpoint-description')?.toLowerCase() || '';
            
            const matchesText = endpoint.includes(search);
            const matchesPath = dataPath.includes(search) || endpointData?.path?.toLowerCase().includes(search);
            const matchesDescription = dataDescription.includes(search) || endpointData?.description?.toLowerCase().includes(search);
            const matchesSummary = dataSummary.includes(search) || endpointData?.summary?.toLowerCase().includes(search);
            
            if (matchesText || matchesPath || matchesDescription || matchesSummary) {
                item.style.display = '';
                hasVisibleEndpoint = true;
            } else {
                item.style.display = 'none';
            }
        });
        
        // Hide the controller group if no endpoints match
        group.style.display = hasVisibleEndpoint ? '' : 'none';
    });
}

function generateJsonExample(model, depth = 0, visited = new Set()) {
    if (!model || depth > 3) return null; // Limit depth to avoid infinite recursion
    
    // Check for circular references
    if (model.id && visited.has(model.id)) {
        return '...'; // Circular reference indicator
    }
    
    if (model.id) {
        visited.add(model.id);
    }
    
    // Handle different model types
    if (model.modelType === 'Primitive') {
        // Return example values based on type
        const type = (model.name || model.type || '').toLowerCase();
        if (type.includes('string')) return 'string';
        if (type.includes('int') || type.includes('long') || type.includes('short')) return 0;
        if (type.includes('decimal') || type.includes('double') || type.includes('float')) return 0.0;
        if (type.includes('bool')) return true;
        if (type.includes('datetime') || type.includes('date')) return '2024-01-01T00:00:00Z';
        if (type.includes('guid') || type.includes('uuid')) return '00000000-0000-0000-0000-000000000000';
        return 'value';
    }
    
    if (model.modelType === 'Enum') {
        // Return first enum member value if available
        if (model.enumMembers && model.enumMembers.length > 0) {
            return model.enumMembers[0].value || model.enumMembers[0].name;
        }
        return 0;
    }
    
    if (model.modelType === 'Array' || model.elementType) {
        // Array type - return array with one example element
        const elementExample = model.elementType ? generateJsonExample(model.elementType, depth + 1, new Set(visited)) : 'item';
        return [elementExample];
    }
    
    if (model.modelType === 'Dictionary' || (model.keyType && model.valueType)) {
        // Dictionary type
        const keyExample = model.keyType ? generateJsonExample(model.keyType, depth + 1, new Set(visited)) : 'key';
        const valueExample = model.valueType ? generateJsonExample(model.valueType, depth + 1, new Set(visited)) : 'value';
        return { [keyExample]: valueExample };
    }
    
    if (model.tupleElements && model.tupleElements.length > 0) {
        // Tuple type - return object with tuple elements
        const result = {};
        model.tupleElements.forEach((elem, idx) => {
            const key = elem.name || `item${idx + 1}`;
            const elemModel = elem.modelId ? apiData.models.find(m => m.id === elem.modelId) : null;
            result[key] = elemModel ? generateJsonExample(elemModel, depth + 1, new Set(visited)) : (elem.type || 'value');
        });
        return result;
    }
    
    if (model.fields && model.fields.length > 0) {
        // Object with fields
        const result = {};
        model.fields.forEach(field => {
            const fieldModel = field.modelId ? apiData.models.find(m => m.id === field.modelId) : null;
            if (fieldModel) {
                result[field.name] = generateJsonExample(fieldModel, depth + 1, new Set(visited));
            } else {
                // Generate example based on field type
                const type = (field.type || '').toLowerCase();
                if (type.includes('string')) result[field.name] = field.example || 'string';
                else if (type.includes('int') || type.includes('long')) result[field.name] = 0;
                else if (type.includes('decimal') || type.includes('double') || type.includes('float')) result[field.name] = 0.0;
                else if (type.includes('bool')) result[field.name] = true;
                else if (type.includes('datetime') || type.includes('date')) result[field.name] = '2024-01-01T00:00:00Z';
                else if (type.includes('guid')) result[field.name] = '00000000-0000-0000-0000-000000000000';
                else result[field.name] = field.example || null;
            }
        });
        return result;
    }
    
    if (model.genericArguments && model.genericArguments.length > 0) {
        // Generic type with arguments - try to generate from first argument
        return generateJsonExample(model.genericArguments[0], depth + 1, new Set(visited));
    }
    
    return null;
}
