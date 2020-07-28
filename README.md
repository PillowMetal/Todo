# Todo
Sample .NET Core 3 API to manage a list of to do items.

This implementation satisfies all REST constraints as well as Richardson Maturity Model level 3, and provides examples of OPTIONS, HEAD, GET, POST, PUT, PATCH, and DELETE. All wirite methods also support "upserting".

In addition is uses proper identification of resources and methods, an outer facing contract, provides proper error responses (client errors vs. server faults), and contains self-discoverability.

# Features
1. Filtering for specific keys in the outer contract
2. Searching via a searchQuery parameter on the underlying data
3. Sorting via an orderBy query parameter that maps outer facing contract properties to underlying data column(s)
4. Paging via pageSize and pageNumber query parameters with information returned in an X-Pagaination header in the response
5. Data Shaping via a fields query parameter that can be used on either the outer facing contract or underlying data
6. HATEOAS support for both single items and collections via a custom vender media type in the request Accept header
7. Content negotiation for outer facing data or underlying data via custom media type, with or without HATEOAS links
8. Caching at the server level
9. Compression support for both Gzip and Brotli
