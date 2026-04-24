$(function () {
    var PAGE_SIZE   = 12;
    var currentPage = 1;

    // ── Collect filtered cards ────────────────────────────────────────────────

    function getFiltered() {
        var search     = $('#filterSearch').val().toLowerCase().trim();
        var instructor = $('#filterInstructor').val().toLowerCase().trim();
        var minPrice   = parseFloat($('#filterMinPrice').val()) || 0;
        var maxPrice   = parseFloat($('#filterMaxPrice').val()) || Infinity;

        return $('.class-card-col').filter(function () {
            var name  = $(this).data('name').toLowerCase();
            var inst  = $(this).data('instructor').toLowerCase();
            var price = parseFloat($(this).data('price'));
            return name.includes(search) &&
                   (instructor === '' || inst === instructor) &&
                   price >= minPrice && price <= maxPrice;
        });
    }

    // ── Render one page of results ────────────────────────────────────────────

    function renderPage() {
        var filtered   = getFiltered();
        var total      = filtered.length;
        var totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

        if (currentPage > totalPages) currentPage = totalPages;

        // hide all, then show current page slice
        $('.class-card-col').hide();
        var start = (currentPage - 1) * PAGE_SIZE;
        filtered.slice(start, start + PAGE_SIZE).show();

        $('#noResultsAlert').toggle(total === 0);
        renderPagination(totalPages, total);
    }

    // ── Build Bootstrap pagination controls ───────────────────────────────────

    function renderPagination(totalPages, total) {
        var $wrap = $('#pagination');
        $wrap.empty();

        if (totalPages <= 1) {
            if (total > 0) {
                $wrap.html('<p class="text-center text-muted small mt-2">' +
                    total + ' class' + (total !== 1 ? 'es' : '') + '</p>');
            }
            return;
        }

        // page numbers to show (first, last, window of 2 around current)
        var pages = [];
        for (var i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || Math.abs(i - currentPage) <= 2) {
                pages.push(i);
            }
        }

        var html = '<nav aria-label="Class pagination"><ul class="pagination justify-content-center flex-wrap mb-1">';

        // Prev
        html += '<li class="page-item' + (currentPage === 1 ? ' disabled' : '') + '">' +
                '<a class="page-link" href="#" data-page="' + (currentPage - 1) + '">&lsaquo; Prev</a></li>';

        // Pages with ellipsis
        var prev = 0;
        pages.forEach(function (p) {
            if (prev && p - prev > 1) {
                html += '<li class="page-item disabled"><span class="page-link">&hellip;</span></li>';
            }
            html += '<li class="page-item' + (p === currentPage ? ' active' : '') + '">' +
                    '<a class="page-link" href="#" data-page="' + p + '">' + p + '</a></li>';
            prev = p;
        });

        // Next
        html += '<li class="page-item' + (currentPage === totalPages ? ' disabled' : '') + '">' +
                '<a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next &rsaquo;</a></li>';

        html += '</ul></nav>';
        html += '<p class="text-center text-muted small">' +
                total + ' class' + (total !== 1 ? 'es' : '') + ' &bull; page ' +
                currentPage + ' of ' + totalPages + '</p>';

        $wrap.html(html);

        $wrap.find('a.page-link').on('click', function (e) {
            e.preventDefault();
            var page = parseInt($(this).data('page'));
            if (!page || page < 1 || page > totalPages) return;
            currentPage = page;
            renderPage();
            $('html, body').animate({
                scrollTop: $('.class-card-col').first().closest('.row').offset().top - 16
            }, 180);
        });
    }

    // ── Filter inputs reset page to 1 ─────────────────────────────────────────

    function applyFilters() {
        currentPage = 1;
        renderPage();
    }

    $('#filterSearch, #filterInstructor, #filterMinPrice, #filterMaxPrice').on('input change', applyFilters);

    $('#clearFilters').on('click', function () {
        $('#filterSearch').val('');
        $('#filterInstructor').val('');
        $('#filterMinPrice').val('');
        $('#filterMaxPrice').val('');
        applyFilters();
    });

    // initial render
    renderPage();
});
