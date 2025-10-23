# API UI 实现方式代码对比

## 当前实现 vs 推荐方案

### 当前实现（C# 字符串生成）

```csharp
// AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs
public class ApiDocumentationPageService : IApiDocumentationPageService
{
    public string GenerateHtml(ApiDocument document)
    {
        var jsonData = JsonSerializer.Serialize(document, ...);

        return $@"<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
    <meta charset=""UTF-8"" />
    <title>{document.Title} - API Documentation</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" ...>
    <style>
        :root {{
            --bg-primary: #f8f9fa;
            --bg-secondary: #fff;
            /* ...900+ 行样式... */
        }}
    </style>
</head>
<body>
    <!-- ...500+ 行 HTML... -->
    <script>
        const apiData = {jsonData};
        /* ...400+ 行 JavaScript... */
    </script>
</body>
</html>";
    }
}
```

**问题**:
- ❌ 960 行代码混在一个字符串中
- ❌ 无语法高亮
- ❌ 无格式化支持
- ❌ 调试困难
- ❌ 扩展困难

---

### 方案一：Razor Pages（推荐）

#### 项目结构
```
AspNetCore.OpenApi.Xml.UI/                    # 新 RCL 项目
├── Pages/
│   ├── ApiDocumentation.cshtml               # HTML 模板
│   └── ApiDocumentation.cshtml.cs            # Page Model
├── wwwroot/
│   ├── css/
│   │   └── api-doc.css                       # 样式（400 行）
│   └── js/
│       └── api-doc.js                        # JavaScript（500 行）
└── AspNetCore.OpenApi.Xml.UI.csproj
```

#### 代码示例

**1. Page Model (C#)**
```csharp
// Pages/ApiDocumentation.cshtml.cs
public class ApiDocumentationModel : PageModel
{
    private readonly IApiXmlDocumentGenerator _generator;
    
    [BindProperty(SupportsGet = true)]
    public string? Title { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? Version { get; set; }
    
    public ApiDocument Document { get; private set; } = null!;
    
    public ApiDocumentationModel(IApiXmlDocumentGenerator generator)
    {
        _generator = generator;
    }
    
    public void OnGet()
    {
        Document = _generator.Generate(
            Title ?? "API Documentation", 
            Version ?? "1.0"
        );
    }
}
```

**2. Razor View (HTML)**
```cshtml
@* Pages/ApiDocumentation.cshtml *@
@page "/api-doc"
@model ApiDocumentationModel
@{
    Layout = null;
    var jsonData = JsonSerializer.Serialize(Model.Document, new JsonSerializerOptions 
    { 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@Model.Document.Title - API Documentation</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" 
          rel="stylesheet" 
          integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" 
          crossorigin="anonymous">
    <link rel="stylesheet" 
          href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="~/css/api-doc.css" asp-append-version="true" />
</head>
<body>
    <div class="app-container">
        <aside class="sidebar">
            <div class="sidebar-header">
                <h1>@Model.Document.Title</h1>
                <div class="sidebar-version">
                    <i class="bi bi-tag"></i> <span data-i18n="version">Version</span>: @Model.Document.Version
                </div>
                <div class="header-controls">
                    <button class="theme-toggle" id="themeToggle" title="Toggle theme">
                        <i class="bi bi-moon-fill"></i>
                    </button>
                    <button class="lang-toggle" id="langToggle" title="Switch language">EN</button>
                </div>
            </div>
            <div id="api-list" class="p-0"></div>
        </aside>
        <main class="main-content">
            <div class="empty-state" id="empty-state">
                <i class="bi bi-file-earmark-text"></i>
                <h2 data-i18n="apiDoc">API 文档</h2>
                <p class="text-muted" data-i18n="selectEndpoint">请从左侧选择一个接口以查看详细信息</p>
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

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" 
            integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" 
            crossorigin="anonymous"></script>
    <script src="~/js/api-doc.js" asp-append-version="true"></script>
    <script>
        // 注入 API 数据
        const apiData = @Html.Raw(jsonData);
    </script>
</body>
</html>
```

**3. CSS 文件**
```css
/* wwwroot/css/api-doc.css */
:root {
    --bg-primary: #f8f9fa;
    --bg-secondary: #fff;
    --bg-tertiary: #e9ecef;
    --bg-header: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
    /* ...其他 CSS 变量... */
}

[data-theme="dark"] {
    --bg-primary: #1a1d20;
    --bg-secondary: #212529;
    /* ...深色主题... */
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    background: var(--bg-primary);
    color: var(--text-primary);
    transition: background-color 0.3s, color 0.3s;
}

/* ...其他样式（约 400 行）... */
```

**4. JavaScript 文件**
```javascript
// wwwroot/js/api-doc.js
let typeModal;
let currentLang = 'zh-CN';

// Localization strings
const i18n = {
    'zh-CN': {
        'version': '版本',
        'apiDoc': 'API 文档',
        // ...其他翻译...
    },
    'en-US': {
        'version': 'Version',
        'apiDoc': 'API Documentation',
        // ...其他翻译...
    }
};

function t(key) {
    return i18n[currentLang]?.[key] || i18n['en-US']?.[key] || key;
}

document.addEventListener('DOMContentLoaded', function() {
    typeModal = new bootstrap.Modal(document.getElementById('typeModal'));
    
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const theme = savedTheme || (prefersDark ? 'dark' : 'light');
    setTheme(theme);

    const savedLang = localStorage.getItem('lang');
    const browserLang = navigator.language;
    currentLang = savedLang || (browserLang.startsWith('zh') ? 'zh-CN' : 'en-US');
    document.getElementById('langToggle').textContent = currentLang === 'zh-CN' ? 'EN' : '中文';
    
    renderApiList();
    updateLocalization();

    // Event listeners
    document.getElementById('themeToggle').addEventListener('click', function() {
        const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
    });

    document.getElementById('langToggle').addEventListener('click', function() {
        currentLang = currentLang === 'zh-CN' ? 'en-US' : 'zh-CN';
        localStorage.setItem('lang', currentLang);
        this.textContent = currentLang === 'zh-CN' ? 'EN' : '中文';
        updateLocalization();
    });
});

function renderApiList() {
    // ...实现逻辑（约 500 行）...
}

function showEndpoint(endpoint) {
    // ...实现逻辑...
}

// ...其他函数...
```

**5. 服务注册**
```csharp
// AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApiXmlDocumentGenerator(this IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSingleton<IXmlDocumentationReader, XmlDocumentationReader>();
    services.AddSingleton<IApiXmlDocumentGenerator, ApiXmlDocumentGenerator>();
    
    // 添加 Razor Pages 支持
    services.AddRazorPages();
    
    return services;
}
```

**6. 路由映射**
```csharp
// AspNetCore.OpenApi.Xml/Extensions/EndpointRouteBuilderExtensions.cs
public static IEndpointRouteBuilder MapApiDocumentationPage(
    this IEndpointRouteBuilder endpoints,
    string? title = null,
    string? version = null)
{
    // 映射 Razor Pages
    endpoints.MapRazorPages();
    
    return endpoints;
}
```

**7. 使用示例**
```csharp
// Demo.WebApi/Program.cs
using AspNetCore.OpenApi.Xml.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiXmlDocumentGenerator();  // 自动注册 Razor Pages

var app = builder.Build();

app.MapApiDocumentationPage();  // 自动映射 /api-doc

app.MapControllers();
app.Run();
```

**优点**:
- ✅ HTML/CSS/JS 完全分离
- ✅ 语法高亮和智能提示
- ✅ 独立的 CSS 和 JS 文件，易于维护
- ✅ 可以使用 Razor Tag Helpers
- ✅ 静态资源版本控制（`asp-append-version`）
- ✅ 保持技术栈统一（纯 .NET）
- ✅ 零额外依赖

---

## 方案二：Angular（备选长期方案）

### 项目结构
```
AspNetCore.OpenApi.Xml.UI/                    # Angular 项目
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   ├── api-list/
│   │   │   │   ├── api-list.component.ts
│   │   │   │   ├── api-list.component.html
│   │   │   │   └── api-list.component.scss
│   │   │   ├── endpoint-detail/
│   │   │   │   ├── endpoint-detail.component.ts
│   │   │   │   ├── endpoint-detail.component.html
│   │   │   │   └── endpoint-detail.component.scss
│   │   │   └── type-modal/
│   │   │       ├── type-modal.component.ts
│   │   │       ├── type-modal.component.html
│   │   │       └── type-modal.component.scss
│   │   ├── services/
│   │   │   └── api-document.service.ts
│   │   ├── models/
│   │   │   ├── api-document.model.ts
│   │   │   ├── endpoint.model.ts
│   │   │   └── api-schema.model.ts
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   └── app.component.scss
│   ├── assets/
│   │   └── i18n/
│   │       ├── en-US.json
│   │       └── zh-CN.json
│   ├── styles.scss
│   └── index.html
├── package.json
├── tsconfig.json
└── angular.json
```

### 代码示例

**1. TypeScript 模型**
```typescript
// src/app/models/api-document.model.ts
export interface ApiDocument {
  title: string;
  version: string;
  endpoints: Endpoint[];
  models: ApiModel[];
}

export interface Endpoint {
  operationId: string;
  path: string;
  method: string;
  summary?: string;
  description?: string;
  tags?: string[];
  deprecated: boolean;
  request?: ApiRequest;
  responses?: ApiResponse[];
}

export interface ApiRequest {
  routeParameters?: ApiField[];
  queryParameters?: ApiField[];
  headers?: ApiField[];
  body?: ApiSchema;
  description?: string;
}

export interface ApiResponse {
  statusCode: number;
  contentType?: string;
  description?: string;
  body?: ApiSchema;
}

export interface ApiSchema {
  id: string;
  name: string;
  type: string;
  nullable: boolean;
  description?: string;
  fields?: ApiField[];
  elementType?: ApiSchema;
  // ...其他属性
}

export interface ApiField {
  name: string;
  type: string;
  required: boolean;
  description?: string;
  modelId?: string;
  // ...其他属性
}
```

**2. Angular Service**
```typescript
// src/app/services/api-document.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiDocument, Endpoint } from '../models/api-document.model';

@Injectable({
  providedIn: 'root'
})
export class ApiDocumentService {
  private apiDataSubject = new BehaviorSubject<ApiDocument | null>(null);
  private selectedEndpointSubject = new BehaviorSubject<Endpoint | null>(null);
  
  public apiData$ = this.apiDataSubject.asObservable();
  public selectedEndpoint$ = this.selectedEndpointSubject.asObservable();
  
  constructor(private http: HttpClient) {}
  
  loadDocument(): Observable<ApiDocument> {
    return this.http.get<ApiDocument>('/api/document-json').pipe(
      tap(data => this.apiDataSubject.next(data))
    );
  }
  
  selectEndpoint(endpoint: Endpoint): void {
    this.selectedEndpointSubject.next(endpoint);
  }
  
  getModelById(modelId: string): ApiModel | undefined {
    const doc = this.apiDataSubject.value;
    return doc?.models.find(m => m.id === modelId);
  }
}
```

**3. API List Component**
```typescript
// src/app/components/api-list/api-list.component.ts
import { Component, OnInit } from '@angular/core';
import { ApiDocumentService } from '../../services/api-document.service';
import { Endpoint } from '../../models/api-document.model';

@Component({
  selector: 'app-api-list',
  templateUrl: './api-list.component.html',
  styleUrls: ['./api-list.component.scss']
})
export class ApiListComponent implements OnInit {
  groupedEndpoints: Map<string, Endpoint[]> = new Map();
  
  constructor(private apiService: ApiDocumentService) {}
  
  ngOnInit(): void {
    this.apiService.apiData$.subscribe(data => {
      if (data) {
        this.groupEndpoints(data.endpoints);
      }
    });
  }
  
  private groupEndpoints(endpoints: Endpoint[]): void {
    const groups = new Map<string, Endpoint[]>();
    
    endpoints.forEach(endpoint => {
      const tag = endpoint.tags && endpoint.tags.length > 0 
        ? endpoint.tags[0] 
        : 'Default';
      
      if (!groups.has(tag)) {
        groups.set(tag, []);
      }
      groups.get(tag)!.push(endpoint);
    });
    
    this.groupedEndpoints = groups;
  }
  
  selectEndpoint(endpoint: Endpoint): void {
    this.apiService.selectEndpoint(endpoint);
  }
  
  getMethodClass(method: string): string {
    return `method-${method.toLowerCase()}`;
  }
}
```

```html
<!-- src/app/components/api-list/api-list.component.html -->
<div class="api-list">
  <div *ngFor="let group of groupedEndpoints | keyvalue" class="controller-group">
    <mat-expansion-panel>
      <mat-expansion-panel-header>
        <mat-panel-title>
          {{ group.key }}
        </mat-panel-title>
      </mat-expansion-panel-header>
      
      <mat-nav-list>
        <mat-list-item 
          *ngFor="let endpoint of group.value"
          (click)="selectEndpoint(endpoint)"
          class="endpoint-item">
          <span [ngClass]="getMethodClass(endpoint.method)" class="method-badge">
            {{ endpoint.method }}
          </span>
          <span class="endpoint-path">{{ endpoint.summary || endpoint.path }}</span>
        </mat-list-item>
      </mat-nav-list>
    </mat-expansion-panel>
  </div>
</div>
```

**4. Endpoint Detail Component**
```typescript
// src/app/components/endpoint-detail/endpoint-detail.component.ts
import { Component, OnInit } from '@angular/core';
import { ApiDocumentService } from '../../services/api-document.service';
import { Endpoint } from '../../models/api-document.model';
import { MatDialog } from '@angular/material/dialog';
import { TypeModalComponent } from '../type-modal/type-modal.component';

@Component({
  selector: 'app-endpoint-detail',
  templateUrl: './endpoint-detail.component.html',
  styleUrls: ['./endpoint-detail.component.scss']
})
export class EndpointDetailComponent implements OnInit {
  endpoint: Endpoint | null = null;
  
  constructor(
    private apiService: ApiDocumentService,
    private dialog: MatDialog
  ) {}
  
  ngOnInit(): void {
    this.apiService.selectedEndpoint$.subscribe(endpoint => {
      this.endpoint = endpoint;
    });
  }
  
  showTypeModal(modelId: string): void {
    const model = this.apiService.getModelById(modelId);
    if (model) {
      this.dialog.open(TypeModalComponent, {
        data: { model },
        width: '800px'
      });
    }
  }
  
  getStatusClass(statusCode: number): string {
    if (statusCode >= 200 && statusCode < 300) return 'status-2xx';
    if (statusCode >= 400 && statusCode < 500) return 'status-4xx';
    return 'status-5xx';
  }
}
```

**5. 主应用组件**
```typescript
// src/app/app.component.ts
import { Component, OnInit } from '@angular/core';
import { ApiDocumentService } from './services/api-document.service';
import { ThemeService } from './services/theme.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  template: `
    <mat-sidenav-container class="app-container">
      <mat-sidenav mode="side" opened class="sidebar">
        <div class="sidebar-header">
          <h1>{{ (apiService.apiData$ | async)?.title }}</h1>
          <div class="sidebar-version">
            <mat-icon>tag</mat-icon>
            {{ 'VERSION' | translate }}: {{ (apiService.apiData$ | async)?.version }}
          </div>
          <div class="header-controls">
            <button mat-icon-button (click)="toggleTheme()">
              <mat-icon>{{ isDark ? 'light_mode' : 'dark_mode' }}</mat-icon>
            </button>
            <button mat-stroked-button (click)="toggleLanguage()">
              {{ currentLang === 'zh-CN' ? 'EN' : '中文' }}
            </button>
          </div>
        </div>
        <app-api-list></app-api-list>
      </mat-sidenav>
      
      <mat-sidenav-content class="main-content">
        <app-endpoint-detail></app-endpoint-detail>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isDark = false;
  currentLang = 'zh-CN';
  
  constructor(
    public apiService: ApiDocumentService,
    private themeService: ThemeService,
    private translate: TranslateService
  ) {
    this.translate.setDefaultLang('zh-CN');
    this.translate.use('zh-CN');
  }
  
  ngOnInit(): void {
    this.apiService.loadDocument().subscribe();
    this.isDark = this.themeService.isDarkTheme();
  }
  
  toggleTheme(): void {
    this.isDark = !this.isDark;
    this.themeService.setTheme(this.isDark ? 'dark' : 'light');
  }
  
  toggleLanguage(): void {
    this.currentLang = this.currentLang === 'zh-CN' ? 'en-US' : 'zh-CN';
    this.translate.use(this.currentLang);
  }
}
```

**6. ASP.NET Core 集成**
```csharp
// Program.cs
builder.Services.AddApiXmlDocumentGenerator();

var app = builder.Build();

// 提供 JSON API
app.MapGet("/api/document-json", (IApiXmlDocumentGenerator gen) =>
{
    var document = gen.Generate("API Documentation", "1.0");
    return Results.Json(document, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
});

// 提供 Angular 静态文件
app.UseStaticFiles();
app.MapFallbackToFile("/api-doc/{**path}", "api-ui/index.html");

app.MapControllers();
app.Run();
```

**7. 构建脚本**
```json
// package.json
{
  "name": "aspnetcore-openapi-xml-ui",
  "version": "1.0.0",
  "scripts": {
    "start": "ng serve",
    "build": "ng build --configuration production --output-path ../AspNetCore.OpenApi.Xml/wwwroot/api-ui",
    "test": "ng test",
    "lint": "ng lint"
  },
  "dependencies": {
    "@angular/core": "^17.0.0",
    "@angular/material": "^17.0.0",
    "@ngx-translate/core": "^15.0.0",
    "rxjs": "^7.8.0"
  }
}
```

---

## 对比总结

| 维度 | 当前实现 | 方案一：Razor Pages | 方案二：Angular |
|------|----------|---------------------|-----------------|
| **代码组织** | ❌ 单个 960 行字符串 | ✅ 分离为多个文件 | ✅ 完全模块化 |
| **语法高亮** | ❌ 无 | ✅ 完整支持 | ✅ 完整支持 |
| **开发体验** | ❌ 很差 | ✅ 良好 | ✅ 优秀 |
| **维护成本** | ❌ 高 | ✅ 中 | ⚠️ 中-高 |
| **测试能力** | ❌ 无法测试 | ⚠️ 集成测试 | ✅ 单元测试 + E2E |
| **技术栈** | .NET | ✅ .NET | ⚠️ .NET + Node.js |
| **学习成本** | - | ✅ 低 | ⚠️ 高 |
| **迁移时间** | - | ✅ 2-3 天 | ⚠️ 8-12 天 |
| **扩展性** | ❌ 差 | ✅ 中 | ✅ 优秀 |
| **构建依赖** | 无 | ✅ 无 | ⚠️ Node.js/npm |
| **部署复杂度** | 简单 | ✅ 简单 | ⚠️ 中等 |

---

## 推荐结论

**立即采用：方案一（Razor Pages）**

原因：
1. ✅ 快速解决当前维护性问题（2-3 天）
2. ✅ 保持技术栈统一（纯 .NET）
3. ✅ 代码组织大幅改善（HTML/CSS/JS 分离）
4. ✅ 零额外依赖和构建步骤
5. ✅ 适合当前项目规模和复杂度

**未来考虑：方案二（Angular）**

时机：
- 当需要添加复杂交互（API 在线测试、Mock 服务器）
- 或项目演进为独立产品时
- 或团队有专职前端开发人员时
