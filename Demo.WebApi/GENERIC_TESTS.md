# Generic Types and Dictionary Tests

This document lists all the test endpoints added to the Demo.WebApi project for testing generic types and dictionaries in the API documentation generator.

## Controller: GenericsTestController

### Generic Types with Multiple Type Parameters

#### 1. KeyValuePair<TKey, TValue>
- **GET** `/api/GenericsTest/kvp`
  - Returns: `KeyValuePair<string, int>`
  - Example: `{ "key": "count", "value": 42 }`

- **GET** `/api/GenericsTest/kvp-list`
  - Returns: `List<KeyValuePair<string, UserDto>>`
  - Tests: List of KeyValuePairs with complex value types

#### 2. Tuple Types
- **GET** `/api/GenericsTest/tuple2`
  - Returns: `Tuple<string, int>`
  - Example: `{ "item1": "example", "item2": 123 }`

- **GET** `/api/GenericsTest/tuple3`
  - Returns: `Tuple<string, int, DateTime>`
  - Tests: Tuple with 3 type parameters

- **GET** `/api/GenericsTest/value-tuple`
  - Returns: `(string Name, int Id, DateTime CreatedAt)`
  - Tests: ValueTuple with named fields

#### 3. Custom Generic Types
- **GET** `/api/GenericsTest/result`
  - Returns: `Result<UserDto, string>`
  - Tests: Custom generic type with 2 type parameters for success/error scenarios

- **GET** `/api/GenericsTest/triple`
  - Returns: `Triple<int, string, UserDto>`
  - Tests: Custom generic type with 3 type parameters

- **POST** `/api/GenericsTest/paged-result`
  - Input: `PageRequest` (page, pageSize)
  - Returns: `PagedResult<UserDto>`
  - Tests: Generic wrapper for paged data with metadata

### Dictionary Types

#### 4. Basic Dictionary
- **POST** `/api/GenericsTest/dict-input`
  - Input: `Dictionary<string, string>`
  - Returns: `Dictionary<string, string>`
  - Tests: Dictionary as both input and output

#### 5. Nested Dictionary
- **GET** `/api/GenericsTest/nested-dict`
  - Returns: `Dictionary<string, Dictionary<string, int>>`
  - Tests: Nested dictionary structures

#### 6. Dictionary with Complex Value Types
- **GET** `/api/GenericsTest/dict-complex`
  - Returns: `Dictionary<int, UserDetailDto>`
  - Tests: Dictionary with complex objects as values

#### 7. Dictionary with Tuple Values
- **GET** `/api/GenericsTest/dict-tuple`
  - Returns: `Dictionary<string, (int Count, string Status)>`
  - Tests: Dictionary with ValueTuple values

#### 8. Dictionary with Enum Keys
- **GET** `/api/GenericsTest/dict-enum-key`
  - Returns: `Dictionary<RoleKind, List<UserDto>>`
  - Tests: Dictionary with enum keys and list values

## Testing the Endpoints

You can test all endpoints using curl:

```bash
# Test KeyValuePair
curl http://localhost:5555/api/GenericsTest/kvp

# Test Tuple
curl http://localhost:5555/api/GenericsTest/tuple2

# Test nested dictionary
curl http://localhost:5555/api/GenericsTest/nested-dict

# Test paged result
curl -X POST http://localhost:5555/api/GenericsTest/paged-result \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 10}'

# Test dictionary input
curl -X POST http://localhost:5555/api/GenericsTest/dict-input \
  -H "Content-Type: application/json" \
  -d '{"key1": "value1", "key2": "value2"}'
```

## Coverage Summary

The test endpoints cover:
- ✅ Generic types with 2 type parameters (KeyValuePair, Result, Tuple)
- ✅ Generic types with 3 type parameters (Triple, Tuple)
- ✅ List of generic types
- ✅ Custom generic wrappers (PagedResult)
- ✅ Dictionary with string keys and values
- ✅ Dictionary with integer keys
- ✅ Dictionary with enum keys
- ✅ Nested dictionaries
- ✅ Dictionary with complex object values
- ✅ Dictionary with tuple values
- ✅ Dictionary as input parameter
