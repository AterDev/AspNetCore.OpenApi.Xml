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
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: #f8f9fa;
        }}

        .app-container {{
            display: flex;
            height: 100vh;
        }}

        .sidebar {{
            width: 320px;
            background: #fff;
            border-right: 1px solid #dee2e6;
            overflow-y: auto;
            flex-shrink: 0;
        }}

        .sidebar-header {{
            padding: 1.5rem 1.25rem;
            border-bottom: 2px solid #0d6efd;
            background: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
            color: white;
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

        .main-content {{
            flex: 1;
            overflow-y: auto;
            padding: 2rem;
        }}

        .controller-group {{
            margin-bottom: 0.5rem;
        }}

        .controller-header {{
            background: #f8f9fa;
            padding: 0.75rem 1.25rem;
            cursor: pointer;
            border-left: 3px solid #6c757d;
            transition: all 0.2s;
            display: flex;
            justify-content: space-between;
            align-items: center;
            user-select: none;
        }}

        .controller-header:hover {{
            background: #e9ecef;
            border-left-color: #0d6efd;
        }}

        .controller-header.active {{
            border-left-color: #0d6efd;
            background: #e7f1ff;
        }}

        .controller-name {{
            font-weight: 600;
            font-size: 0.95rem;
            color: #212529;
        }}

        .controller-chevron {{
            transition: transform 0.2s;
            color: #6c757d;
        }}

        .controller-header.collapsed .controller-chevron {{
            transform: rotate(-90deg);
        }}

        .endpoint-list {{
            background: #fff;
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
            background: #f8f9fa;
            border-left-color: #0d6efd;
        }}

        .endpoint-item.active {{
            background: #e7f1ff;
            border-left-color: #0d6efd;
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
            color: #495057;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }}

        .empty-state {{
            text-align: center;
            padding: 4rem 2rem;
            color: #6c757d;
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
            background: #f8f9fa;
            border-left: 3px solid #0d6efd;
            padding: 0.75rem;
            margin-bottom: 0.5rem;
            border-radius: 0.25rem;
        }}

        .enum-member-name {{
            font-weight: 600;
            font-size: 0.9rem;
            color: #212529;
        }}

        .enum-member-value {{
            color: #6c757d;
            font-size: 0.85rem;
            margin-top: 0.25rem;
        }}

        .validation-text {{
            font-size: 0.85rem;
            color: #6c757d;
            margin-top: 0.25rem;
        }}

        code {{
            background: #f8f9fa;
            padding: 0.125rem 0.375rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            color: #e83e8c;
        }}

        .modal-body table {{
            margin-bottom: 0;
        }}
    </style>
</head>
<body>
    <div class=""app-container"">
        <aside class=""sidebar"">
            <div class=""sidebar-header"">
                <h1>{document.Title}</h1>
                <div class=""sidebar-version"">
                    <i class=""bi bi-tag""></i> Version: {document.Version}
                </div>
            </div>
            <div id=""api-list"" class=""p-0""></div>
        </aside>
        <main class=""main-content"">
            <div class=""empty-state"" id=""empty-state"">
                <i class=""bi bi-file-earmark-text""></i>
                <h2>API 文档</h2>
                <p class=""text-muted"">请从左侧选择一个接口以查看详细信息</p>
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

        document.addEventListener('DOMContentLoaded', function() {{
            typeModal = new bootstrap.Modal(document.getElementById('typeModal'));
            renderApiList();
        }});

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
                    // Prioritize description, then path
                    const displayText = endpoint.description || endpoint.path;
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

            // Use description if available, otherwise use path - avoid showing full controller/method names
            const title = endpoint.description || endpoint.path;
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
                            <div class=""card-header""><i class=""bi bi-signpost""></i> 路由参数 (Route Parameters)</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.routeParameters)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.queryParameters && req.queryParameters.length > 0) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-question-circle""></i> 查询参数 (Query Parameters)</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.queryParameters)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.headers && req.headers.length > 0) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-list-ul""></i> 请求头 (Headers)</div>
                            <div class=""card-body p-0"">
                                ${{renderParamsTable(req.headers)}}
                            </div>
                        </div>
                    `;
                }}

                if (req.body) {{
                    html += `
                        <div class=""card mb-3"">
                            <div class=""card-header""><i class=""bi bi-file-earmark-code""></i> 请求体 (Request Body)</div>
                            <div class=""card-body p-0"">
                                ${{renderModel(req.body)}}
                            </div>
                        </div>
                    `;
                }}
            }}

            if (endpoint.responses && endpoint.responses.length > 0) {{
                html += `<div class=""card mb-3"">
                    <div class=""card-header""><i class=""bi bi-reply""></i> 响应 (Responses)</div>
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
                            ${{resp.body ? renderModel(resp.body) : '<p class=""text-muted mb-0"">无响应体</p>'}}
                        </div>
                    `;
                }});
                html += `</div></div>`;
            }}

            detail.innerHTML = html;
        }}

        function renderParamsTable(params) {{
            if (!params || params.length === 0) {{
                return '<p class=""text-muted p-3 mb-0"">无参数</p>';
            }}

            let html = `
                <table class=""table table-sm table-hover mb-0"">
                    <thead class=""table-light"">
                        <tr>
                            <th style=""width: 25%"">参数名</th>
                            <th style=""width: 20%"">类型</th>
                            <th style=""width: 15%"">必填</th>
                            <th style=""width: 40%"">说明</th>
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
                        <td>${{param.required ? '<span class=""badge bg-danger"">必填</span>' : '<span class=""badge bg-secondary"">可选</span>'}}</td>
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
            if (!model) return '<p class=""text-muted mb-0"">无模型信息</p>';

            let html = '';
            
            if (model.fields && model.fields.length > 0) {{
                html += renderParamsTable(model.fields);
            }} else if (model.elementType) {{
                html += `<p class=""p-3 mb-0"">数组类型: ${{renderModelType(model.elementType)}}</p>`;
            }} else if (model.modelType === 'Enum') {{
                html += `<p class=""p-3 mb-0"">枚举类型</p>`;
            }} else {{
                html += `<p class=""p-3 mb-0"">类型: ${{model.name || model.id || 'unknown'}}</p>`;
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
                alert('模型未找到: ' + modelId);
                return;
            }}

            document.getElementById('typeModalLabel').textContent = model.name || model.id;
            
            let html = `
                <div class=""mb-3"">
                    ${{model.namespace ? `<p class=""mb-1""><strong>命名空间:</strong> ${{model.namespace}}</p>` : ''}}
                    <p class=""mb-1""><strong>类型:</strong> ${{model.modelType}}</p>
                    ${{model.nullable ? '<p class=""mb-1""><strong>可空:</strong> 是</p>' : ''}}
                    ${{model.description ? `<p class=""mb-1""><strong>说明:</strong> ${{model.description}}</p>` : ''}}
                </div>
            `;

            if (model.modelType === 'Enum' && model.enumMembers && model.enumMembers.length > 0) {{
                html += `<h6 class=""mb-3"">枚举值</h6>`;
                model.enumMembers.forEach(member => {{
                    html += `
                        <div class=""enum-member"">
                            <div class=""enum-member-name"">${{member.name}} = ${{member.value}}</div>
                            ${{member.description ? `<div class=""enum-member-value"">${{member.description}}</div>` : ''}}
                        </div>
                    `;
                }});
            }} else if (model.fields && model.fields.length > 0) {{
                html += `<h6 class=""mb-3"">字段</h6>${{renderParamsTable(model.fields)}}`;
            }} else if (model.elementType) {{
                html += `<h6 class=""mb-2"">元素类型</h6><p>${{renderModelType(model.elementType)}}</p>`;
            }} else if (model.keyType && model.valueType) {{
                html += `
                    <h6 class=""mb-2"">字典类型</h6>
                    <p><strong>键类型:</strong> ${{renderModelType(model.keyType)}}</p>
                    <p><strong>值类型:</strong> ${{renderModelType(model.valueType)}}</p>
                `;
            }}

            document.getElementById('typeModalBody').innerHTML = html;
            typeModal.show();
        }}
    </script>
</body>
</html>";
    }
}
