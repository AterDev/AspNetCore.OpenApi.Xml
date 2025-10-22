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
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        return $@"<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
    <meta charset=""UTF-8"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{document.Title} - API Documentation</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH"" crossorigin=""anonymous"">
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"">
    <style>
        :root {{
            --bg-primary: #f8f9fa;
            --bg-secondary: #fff;
            --bg-tertiary: #e9ecef;
            --bg-header: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
            --text-primary: #212529;
            --text-secondary: #6c757d;
            --text-muted: #495057;
            --border-color: #dee2e6;
            --border-color-active: #0d6efd;
            --sidebar-hover: #e9ecef;
            --sidebar-active: #e7f1ff;
            --controller-border: #6c757d;
        }}

        [data-theme=""dark""] {{
            --bg-primary: #1a1d20;
            --bg-secondary: #212529;
            --bg-tertiary: #343a40;
            --bg-header: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
            --text-primary: #f8f9fa;
            --text-secondary: #adb5bd;
            --text-muted: #ced4da;
            --border-color: #495057;
            --border-color-active: #0d6efd;
            --sidebar-hover: #343a40;
            --sidebar-active: #1e3a5f;
            --controller-border: #6c757d;
        }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: var(--bg-primary);
            color: var(--text-primary);
            transition: background-color 0.3s, color 0.3s;
        }}

        .app-container {{
            display: flex;
            height: 100vh;
        }}

        .sidebar {{
            width: 320px;
            background: var(--bg-secondary);
            border-right: 1px solid var(--border-color);
            overflow-y: auto;
            flex-shrink: 0;
        }}

        .sidebar-header {{
            padding: 1.5rem 1.25rem;
            border-bottom: 2px solid var(--border-color-active);
            background: var(--bg-header);
            color: white;
            position: relative;
        }}

        .sidebar-header h1 {{
            font-size: 1.25rem;
            margin: 0;
            font-weight: 600;
        }}

        .sidebar-version {{
            font-size: 0.875rem;
            opacity: 0.9;
            margin-top: 0.25rem;
        }}

        .header-controls {{
            position: absolute;
            top: 1rem;
            right: 1rem;
            display: flex;
            gap: 0.5rem;
        }}

        .theme-toggle, .lang-toggle {{
            background: rgba(255, 255, 255, 0.2);
            border: 1px solid rgba(255, 255, 255, 0.3);
            color: white;
            padding: 0.25rem 0.5rem;
            border-radius: 0.25rem;
            cursor: pointer;
            font-size: 0.875rem;
            transition: background 0.2s;
        }}

        .theme-toggle:hover, .lang-toggle:hover {{
            background: rgba(255, 255, 255, 0.3);
        }}

        .main-content {{
            flex: 1;
            overflow-y: auto;
            padding: 2rem;
            background: var(--bg-primary);
        }}

        .controller-group {{
            margin-bottom: 0.5rem;
        }}

        .controller-header {{
            background: var(--bg-tertiary);
            padding: 0.75rem 1.25rem;
            cursor: pointer;
            border-left: 3px solid var(--controller-border);
            transition: all 0.2s;
            display: flex;
            justify-content: space-between;
            align-items: center;
            user-select: none;
        }}

        .controller-header:hover {{
            background: var(--sidebar-hover);
            border-left-color: var(--border-color-active);
        }}

        .controller-header.active {{
            border-left-color: var(--border-color-active);
            background: var(--sidebar-active);
        }}

        .controller-name {{
            font-weight: 600;
            font-size: 0.95rem;
            color: var(--text-primary);
        }}

        .controller-chevron {{
            transition: transform 0.2s;
            color: var(--text-secondary);
        }}

        .controller-header.collapsed .controller-chevron {{
            transform: rotate(-90deg);
        }}

        .endpoint-list {{
            background: var(--bg-secondary);
        }}

        .endpoint-item {{
            padding: 0.625rem 1.25rem 0.625rem 2.5rem;
            cursor: pointer;
            border-left: 3px solid transparent;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }}

        .endpoint-item:hover {{
            background: var(--bg-tertiary);
            border-left-color: var(--border-color-active);
        }}

        .endpoint-item.active {{
            background: var(--sidebar-active);
            border-left-color: var(--border-color-active);
        }}

        .method-badge {{
            font-size: 0.7rem;
            font-weight: 700;
            padding: 0.25rem 0.5rem;
            min-width: 60px;
            text-align: center;
            border-radius: 0.25rem;
        }}

        .method-get {{ background-color: #28a745; color: white; }}
        .method-post {{ background-color: #0d6efd; color: white; }}
        .method-put {{ background-color: #fd7e14; color: white; }}
        .method-delete {{ background-color: #dc3545; color: white; }}
        .method-patch {{ background-color: #6f42c1; color: white; }}

        .endpoint-path {{
            flex: 1;
            font-size: 0.875rem;
            color: var(--text-muted);
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }}

        .empty-state {{
            text-align: center;
            padding: 4rem 2rem;
            color: var(--text-secondary);
        }}

        .empty-state i {{
            font-size: 4rem;
            margin-bottom: 1rem;
            opacity: 0.3;
        }}

        .endpoint-detail-header {{
            margin-bottom: 1.5rem;
        }}

        .endpoint-title {{
            font-size: 1.75rem;
            font-weight: 600;
            margin-bottom: 1rem;
            color: var(--text-primary);
        }}

        .endpoint-meta {{
            display: flex;
            gap: 0.75rem;
            flex-wrap: wrap;
            align-items: center;
        }}

        .type-link {{
            color: #0d6efd;
            cursor: pointer;
            text-decoration: none;
            border-bottom: 1px dashed #0d6efd;
        }}

        .type-link:hover {{
            color: #0a58ca;
            border-bottom-style: solid;
        }}

        .status-badge {{
            font-weight: 600;
            padding: 0.35rem 0.75rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
        }}

        .status-2xx {{ background-color: #d1e7dd; color: #0f5132; }}
        .status-4xx {{ background-color: #fff3cd; color: #664d03; }}
        .status-5xx {{ background-color: #f8d7da; color: #842029; }}

        .enum-member {{
            background: var(--bg-tertiary);
            border-left: 3px solid #0d6efd;
            padding: 0.75rem;
            margin-bottom: 0.5rem;
            border-radius: 0.25rem;
        }}

        .enum-member-name {{
            font-weight: 600;
            font-size: 0.9rem;
            color: var(--text-primary);
        }}

        .enum-member-value {{
            color: var(--text-secondary);
            font-size: 0.85rem;
            margin-top: 0.25rem;
        }}

        .validation-text {{
            font-size: 0.85rem;
            color: var(--text-secondary);
            margin-top: 0.25rem;
        }}

        code {{
            background: var(--bg-tertiary);
            padding: 0.125rem 0.375rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            color: #e83e8c;
        }}

        .modal-body table {{
            margin-bottom: 0;
        }}

        .card {{
            background: var(--bg-secondary);
            border-color: var(--border-color);
            color: var(--text-primary);
        }}

        .card-header {{
            background: var(--bg-tertiary);
            border-color: var(--border-color);
            color: var(--text-primary);
        }}

        .table {{
            color: var(--text-primary);
        }}

        .table-light {{
            background: var(--bg-tertiary);
            color: var(--text-primary);
        }}

        [data-theme=""dark""] .table-hover tbody tr:hover {{
            background-color: var(--bg-tertiary);
        }}

        .modal-content {{
            background: var(--bg-secondary);
            color: var(--text-primary);
        }}

        .modal-header {{
            border-color: var(--border-color);
        }}

        .btn-close {{
            filter: invert(1);
        }}

        [data-theme=""light""] .btn-close {{
            filter: none;
        }}
    </style>
</head>
<body>
    <div class=""app-container"">
        <aside class=""sidebar"">
            <div class=""sidebar-header"">
                <h1>{document.Title}</h1>
                <div class=""sidebar-version"">
                    <i class=""bi bi-tag""></i> <span data-i18n=""version"">Version</span>: {document.Version}
                </div>
                <div class=""header-controls"">
                    <button class=""theme-toggle"" id=""themeToggle"" title=""Toggle theme"">
                        <i class=""bi bi-moon-fill""></i>
                    </button>
                    <button class=""lang-toggle"" id=""langToggle"" title=""Switch language"">EN</button>
                </div>
            </div>
            <div id=""api-list"" class=""p-0""></div>
        </aside>
        <main class=""main-content"">
            <div class=""empty-state"" id=""empty-state"">
                <i class=""bi bi-file-earmark-text""></i>
                <h2 data-i18n=""apiDoc"">API 文档</h2>
                <p class=""text-muted"" data-i18n=""selectEndpoint"">请从左侧选择一个接口以查看详细信息</p>
            </div>
            <div id=""endpoint-detail"" style=""display: none;""></div>
        </main>
    </div>

    <!-- Type Modal -->
    <div class=""modal fade"" id=""typeModal"" tabindex=""-1"" aria-labelledby=""typeModalLabel"" aria-hidden=""true"">
        <div class=""modal-dialog modal-lg modal-dialog-scrollable"">
            <div class=""modal-content"">
                <div class=""modal-header"">
                    <h5 class=""modal-title"" id=""typeModalLabel"">Type Information</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal"" aria-label=""Close""></button>
                </div>
                <div class=""modal-body"" id=""typeModalBody""></div>
            </div>
        </div>
    </div>

    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"" crossorigin=""anonymous""></script>
    <script>
        const apiData = {jsonData};
        let typeModal;
        let currentLang = 'zh-CN';

        // Localization strings
        const i18n = {{
            'zh-CN': {{
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
            }},
            'en-US': {{
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
            }}
        }};

        function t(key) {{
            return i18n[currentLang]?.[key] || i18n['en-US']?.[key] || key;
        }}

        function updateLocalization() {{
            document.querySelectorAll('[data-i18n]').forEach(el => {{
                const key = el.getAttribute('data-i18n');
                el.textContent = t(key);
            }});
            
            // Update empty state text
            const emptyState = document.getElementById('empty-state');
            if (emptyState) {{
                emptyState.querySelector('h2').textContent = t('apiDoc');
                emptyState.querySelector('p').textContent = t('selectEndpoint');
            }}
        }}

        document.addEventListener('DOMContentLoaded', function() {{
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
            document.getElementById('themeToggle').addEventListener('click', function() {{
                const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
                const newTheme = currentTheme === 'light' ? 'dark' : 'light';
                setTheme(newTheme);
            }});

            // Language toggle
            document.getElementById('langToggle').addEventListener('click', function() {{
                currentLang = currentLang === 'zh-CN' ? 'en-US' : 'zh-CN';
                localStorage.setItem('lang', currentLang);
                this.textContent = currentLang === 'zh-CN' ? 'EN' : '中文';
                updateLocalization();
                // Re-render the current endpoint if one is selected
                const activeEndpoint = document.querySelector('.endpoint-item.active');
                if (activeEndpoint) {{
                    const endpointIndex = Array.from(document.querySelectorAll('.endpoint-item')).indexOf(activeEndpoint);
                    const allEndpoints = apiData.endpoints;
                    if (allEndpoints[endpointIndex]) {{
                        showEndpoint(allEndpoints[endpointIndex]);
                    }}
                }}
            }});
        }});

        function setTheme(theme) {{
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem('theme', theme);
            const icon = document.querySelector('#themeToggle i');
            if (theme === 'dark') {{
                icon.className = 'bi bi-sun-fill';
            }} else {{
                icon.className = 'bi bi-moon-fill';
            }}
        }}

        function renderApiList() {{
            const groupedEndpoints = {{}};
            apiData.endpoints.forEach(endpoint => {{
                const tag = endpoint.tags && endpoint.tags.length > 0 ? endpoint.tags[0] : 'Default';
                if (!groupedEndpoints[tag]) {{
                    groupedEndpoints[tag] = [];
                }}
                groupedEndpoints[tag].push(endpoint);
            }});

            const apiList = document.getElementById('api-list');
            Object.keys(groupedEndpoints).sort().forEach(controller => {{
                const groupDiv = document.createElement('div');
                groupDiv.className = 'controller-group';

                const headerDiv = document.createElement('div');
                headerDiv.className = 'controller-header';
                headerDiv.innerHTML = `
                    <span class=""controller-name"">${{controller}}</span>
                    <i class=""bi bi-chevron-down controller-chevron""></i>
                `;

                const collapse = document.createElement('div');
                collapse.className = 'collapse show endpoint-list';
                collapse.id = `collapse-${{controller.replace(/[^a-zA-Z0-9]/g, '-')}}`;

                groupedEndpoints[controller].forEach(endpoint => {{
                    const item = document.createElement('div');
                    item.className = 'endpoint-item';
                    // Prioritize summary (XML comment), then description, then path
                    const displayText = endpoint.summary || endpoint.description || endpoint.path;
                    item.innerHTML = `
                        <span class=""badge method-badge method-${{endpoint.method.toLowerCase()}}"">${{endpoint.method}}</span>
                        <span class=""endpoint-path"">${{displayText}}</span>
                    `;
                    item.onclick = () => {{
                        document.querySelectorAll('.endpoint-item').forEach(i => i.classList.remove('active'));
                        item.classList.add('active');
                        showEndpoint(endpoint);
                    }};
                    collapse.appendChild(item);
                }});

                headerDiv.onclick = () => {{
                    headerDiv.classList.toggle('collapsed');
                    const bsCollapse = new bootstrap.Collapse(collapse, {{toggle: true}});
                }};

                groupDiv.appendChild(headerDiv);
                groupDiv.appendChild(collapse);
                apiList.appendChild(groupDiv);
            }});
        }}

        function showEndpoint(endpoint) {{
            document.getElementById('empty-state').style.display = 'none';
            const detail = document.getElementById('endpoint-detail');
            detail.style.display = 'block';

            // Use summary (XML comment) if available, otherwise use description or path
            const title = endpoint.summary || endpoint.description || endpoint.path;
            let html = `
                <div class=""endpoint-detail-header"">
                    <h2 class=""endpoint-title"">${{title}}</h2>
                    <div class=""endpoint-meta"">
                        <span class=""badge method-badge method-${{endpoint.method.toLowerCase()}}"">${{endpoint.method}}</span>
                        <code>${{endpoint.path}}</code>
                        ${{endpoint.deprecated ? '<span class=""badge bg-danger"">DEPRECATED</span>' : ''}}
                        ${{endpoint.tags ? endpoint.tags.map(t => `<span class=""badge bg-secondary"">${{t}}</span>`).join('') : ''}}
                    </div>
                </div>
            `;

            if (endpoint.description) {{
                html += `<div class=""card mb-3""><div class=""card-body"">${{endpoint.description}}</div></div>`;
            }}

            if (endpoint.request) {{
                const req = endpoint.request;
                
                if (req.routeParameters && req.routeParameters.length > 0) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-signpost""></i> ${{t('routeParams')}}</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.routeParameters)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.queryParameters && req.queryParameters.length > 0) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-question-circle""></i> ${{t('queryParams')}}</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.queryParameters)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.headers && req.headers.length > 0) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-list-ul""></i> ${{t('headers')}}</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.headers)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.body) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-file-earmark-code""></i> ${{t('requestBody')}}</div>
                            <div class=""card-body p-0"">
                                ${{renderModel(req.body)}}
                            </div>
                        </div>
                    `;
                }}
            }}

            if (endpoint.responses && endpoint.responses.length > 0) {{
                html += `<div class=""card mb-3"">
                    <div class=""card-header""><i class=""bi bi-reply""></i> ${{t('responses')}}</div>
                    <div class=""card-body"">`;
                endpoint.responses.forEach(resp => {{
                    const statusClass = resp.statusCode >= 200 && resp.statusCode < 300 ? 'status-2xx' :
                                       resp.statusCode >= 400 && resp.statusCode < 500 ? 'status-4xx' : 'status-5xx';
                    html += `
                        <div class=""mb-3"">
                            <div class=""mb-2"">
                                <span class=""status-badge ${{statusClass}}"">${{resp.statusCode}}</span>
                                ${{resp.contentType ? `<code class=""ms-2"">${{resp.contentType}}</code>` : ''}}
                            </div>
                            ${{resp.body ? renderModel(resp.body) : `<p class=""text-muted mb-0"">${{t('noResponse')}}</p>`}}
                        </div>
                    `;
                }});
                html += `</div></div>`;
            }}

            detail.innerHTML = html;
        }}

        function renderParamsTable(params) {{
            if (!params || params.length === 0) {{
                return `<p class=""text-muted p-3 mb-0"">${{t('noParams')}}</p>`;
            }}

            let html = `
                <table class=""table table-sm table-hover mb-0"">
                    <thead class=""table-light"">
                        <tr>
                            <th style=""width: 25%"">${{t('paramName')}}</th>
                            <th style=""width: 20%"">${{t('type')}}</th>
                            <th style=""width: 15%"">${{t('required')}}</th>
                            <th style=""width: 40%"">${{t('description')}}</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            params.forEach(param => {{
                const typeDisplay = param.modelId 
                    ? `<span class=""type-link"" onclick=""showTypeModal('${{param.modelId}}')"">${{param.type || param.modelId}}</span>`
                    : (param.type || 'any');
                
                html += `
                    <tr>
                        <td><code>${{param.name}}</code></td>
                        <td>${{typeDisplay}}</td>
                        <td>${{param.required ? `<span class=""badge bg-danger"">${{t('required')}}</span>` : `<span class=""badge bg-secondary"">${{t('optional')}}</span>`}}</td>
                        <td>
                            ${{param.description || '-'}}
                            ${{renderValidation(param)}}
                        </td>
                    </tr>
                `;
            }});

            html += `</tbody></table>`;
            return html;
        }}

        function renderValidation(field) {{
            const validations = [];
            if (field.minLength) validations.push(`最小长度: ${{field.minLength}}`);
            if (field.maxLength) validations.push(`最大长度: ${{field.maxLength}}`);
            if (field.minimum) validations.push(`最小值: ${{field.minimum}}`);
            if (field.maximum) validations.push(`最大值: ${{field.maximum}}`);
            if (field.pattern) validations.push(`模式: ${{field.pattern}}`);
            return validations.length > 0 ? `<div class=""validation-text"">${{validations.join(', ')}}</div>` : '';
        }}

        function renderModel(model) {{
            if (!model) return `<p class=""text-muted mb-0"">${{t('noResponse')}}</p>`;

            let html = '';
            
            if (model.fields && model.fields.length > 0) {{
                html += renderParamsTable(model.fields);
            }} else if (model.elementType) {{
                html += `<p class=""p-3 mb-0"">${{t('arrayType')}}，${{t('elementType')}}: ${{renderModelType(model.elementType)}}</p>`;
            }} else if (model.keyType && model.valueType) {{
                html += `
                    <div class=""p-3"">
                        <p class=""mb-2""><strong>${{t('dictionaryType')}}</strong></p>
                        <p class=""mb-1"">${{t('keyType')}}: ${{renderModelType(model.keyType)}}</p>
                        <p class=""mb-0"">${{t('valueType')}}: ${{renderModelType(model.valueType)}}</p>
                    </div>
                `;
            }} else if (model.tupleElements && model.tupleElements.length > 0) {{
                html += `
                    <div class=""p-3"">
                        <p class=""mb-2""><strong>${{t('tupleType')}}</strong></p>
                        ${{model.tupleElements.map((elem, idx) => 
                            `<p class=""mb-1"">Item${{idx + 1}}: ${{elem.modelId ? renderModelType({{ id: elem.modelId, name: elem.type }}) : (elem.type || 'unknown')}}</p>`
                        ).join('')}}
                    </div>
                `;
            }} else if (model.genericArguments && model.genericArguments.length > 0) {{
                html += `
                    <div class=""p-3"">
                        <p class=""mb-2""><strong>${{t('genericType')}}: ${{model.name || model.id}}</strong></p>
                        <p class=""mb-1"">${{t('genericParams')}}:</p>
                        <ul class=""mb-0"">
                        ${{model.genericArguments.map(arg => 
                            `<li>${{renderModelType(arg)}}</li>`
                        ).join('')}}
                        </ul>
                    </div>
                `;
            }} else if (model.modelType === 'Enum') {{
                html += `<p class=""p-3 mb-0"">${{t('type')}}: ${{model.name || model.id}}</p>`;
            }} else {{
                html += `<p class=""p-3 mb-0"">${{t('type')}}: ${{model.name || model.id || 'unknown'}}</p>`;
            }}

            return html;
        }}

        function renderModelType(model) {{
            if (!model) return 'unknown';
            if (model.id) {{
                return `<span class=""type-link"" onclick=""showTypeModal('${{model.id}}')"">${{model.name || model.id}}</span>`;
            }}
            return model.name || model.id || 'unknown';
        }}

        function showTypeModal(modelId) {{
            const model = apiData.models.find(m => m.id === modelId);
            if (!model) {{
                alert(t('modelNotFound') + ': ' + modelId);
                return;
            }}

            document.getElementById('typeModalLabel').textContent = model.name || model.id;
            
            let html = `
                <div class=""mb-3"">
                    ${{model.namespace ? `<p class=""mb-1""><strong>${{t('namespace')}}:</strong> ${{model.namespace}}</p>` : ''}}
                    <p class=""mb-1""><strong>${{t('type')}}:</strong> ${{model.modelType}}</p>
                    ${{model.nullable ? `<p class=""mb-1""><strong>${{t('nullable')}}:</strong> ${{t('yes')}}</p>` : ''}}
                    ${{model.description ? `<p class=""mb-1""><strong>${{t('description')}}:</strong> ${{model.description}}</p>` : ''}}
                </div>
            `;

            if (model.modelType === 'Enum' && model.enumMembers && model.enumMembers.length > 0) {{
                html += `<h6 class=""mb-3"">${{t('enumValues')}}</h6>`;
                model.enumMembers.forEach(member => {{
                    html += `
                        <div class=""enum-member"">
                            <div class=""enum-member-name"">${{member.name}} = ${{member.value}}</div>
                            ${{member.description ? `<div class=""enum-member-value"">${{member.description}}</div>` : ''}}
                        </div>
                    `;
                }});
            }} else if (model.fields && model.fields.length > 0) {{
                html += `<h6 class=""mb-3"">${{t('fields')}}</h6>${{renderParamsTable(model.fields)}}`;
            }} else if (model.elementType) {{
                html += `<h6 class=""mb-2"">${{t('elementType')}}</h6><p>${{renderModelType(model.elementType)}}</p>`;
            }} else if (model.keyType && model.valueType) {{
                html += `
                    <h6 class=""mb-2"">${{t('dictionaryType')}}</h6>
                    <p><strong>${{t('keyType')}}:</strong> ${{renderModelType(model.keyType)}}</p>
                    <p><strong>${{t('valueType')}}:</strong> ${{renderModelType(model.valueType)}}</p>
                `;
            }} else if (model.tupleElements && model.tupleElements.length > 0) {{
                html += `<h6 class=""mb-3"">${{t('tupleElements')}}</h6>`;
                html += `
                    <table class=""table table-sm"">
                        <thead class=""table-light"">
                            <tr>
                                <th>${{t('name')}}</th>
                                <th>${{t('type')}}</th>
                            </tr>
                        </thead>
                        <tbody>
                `;
                model.tupleElements.forEach(elem => {{
                    const typeDisplay = elem.modelId 
                        ? `<span class=""type-link"" onclick=""showTypeModal('${{elem.modelId}}')"">${{elem.type || elem.modelId}}</span>`
                        : (elem.type || 'unknown');
                    html += `
                        <tr>
                            <td><code>${{elem.name}}</code></td>
                            <td>${{typeDisplay}}</td>
                        </tr>
                    `;
                }});
                html += `</tbody></table>`;
            }} else if (model.genericArguments && model.genericArguments.length > 0) {{
                html += `<h6 class=""mb-3"">${{t('genericParams')}}</h6>`;
                html += `<ul class=""list-group"">`;
                model.genericArguments.forEach((arg, idx) => {{
                    html += `<li class=""list-group-item"">${{idx + 1}}. ${{renderModelType(arg)}}</li>`;
                }});
                html += `</ul>`;
            }}

            document.getElementById('typeModalBody').innerHTML = html;
            typeModal.show();
        }}
    </script>
</body>
</html>";
    }
}
