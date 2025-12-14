$(document).ready(function(){
    // Chuyển đổi nav
    var btn = $('.header-icon-slide-menu');
    btn.click(function(){
        $('.nav-long').toggleClass('hide-show');
        $('.nav-long').toggleClass('delay');
        $('.nav-short').toggleClass('hide-show-1');
        $('.nav-short').toggleClass('delay');
        $('.content').toggleClass('content-warp');
    })
    // Like
    $('.content-action.content-action__react').click(function(){
        $(this).toggleClass('color-blue');
    })
    $('.members-filters__grids-icon').click(function(){
        $('.members-filters__grids-icon').removeClass('active');
        $(this).addClass('active');
    })
    $('.big-grid').click(function(){
        $('.members .list-members').children().addClass('col-lg-4');
    })
    $('.small-grid').click(function(){
        $('.members .list-members').children().removeClass();
        $('.members .list-members').children().addClass('col-lg-3');
    })
    // Focus Iput
    $('.members .members-filters__input').focus(function(){
        $('.members-filters__placeholder').css('top','-14px');
    })
    $('.members .members-filters__input').focusout(function() {
        if( !$(this).val())
            $('.members-filters__placeholder').css('top','8px');

    })
    // Seacher Members
    $('.members .members-filters__input').on("keyup", function(){
        var values = $(this).val().toLowerCase();
        $('.members .list-members__member').parent().filter(function(){
            $(this).toggle($(this).children().children('.list-members__info').text().toLowerCase().indexOf(values) > -1);
            
            if( $(this).children().children('.list-members__info').text().toLowerCase().indexOf(values) > -1)
            {
                $(this).unhighlight();
                $(this).children().children('.list-members__info').highlight(values);
                return true;
            }
            else
                return false;
        });
    })
    // sort Member
    
    var mylist = $('.members .list-members__name-link');
    var listitems = mylist.children('.members .list-members__name');
    listitems.sort(function(a, b) {
        return $(a).text().toUpperCase().localeCompare($(b).text().toUpperCase());
    })
    listitems.each(function(){
        $('.members .list-members').append($(this).parent().parent().parent().parent())
    })
    $('.members-filters__section').change(function(){
        var chose = $(this).find(":selected").val();
        if (chose == 1) {
            var mylist = $('.members .list-members__name-link');
            var listitems = mylist.children('.members .list-members__name');
            listitems.sort(function(a, b) {
                return $(a).text().toUpperCase().localeCompare($(b).text().toUpperCase());
            })
            listitems.each(function(){
                $('.members .list-members').append($(this).parent().parent().parent().parent())
            })
        }
        if (chose == 2) {
            var mylist = $('.members .list-members__post');
            var listitems = mylist.children('.members .posts-numbers');
            listitems.sort(function(a, b) {
                return + $(b).text() - + $(a).text();
            })
            listitems.each(function(){
                $('.members .list-members').append($(this).parent().parent().parent().parent())
            })
        }
        if (chose == 3) {
            var mylist = $('.members .list-members__post');
            var listitems = mylist.children('.members .number-friend');
            listitems.sort(function(a, b) {
                return + $(b).text() - + $(a).text();
            })
            listitems.each(function(){
                $('.members .list-members').append($(this).parent().parent().parent().parent())
            })
        }
        
    })
    //Chuyển đổi các trang trên thanh nav
    $('.header-logo-link').click(function(){
        $('.main').addClass('hidden');
        $('.home').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('.nav-short__item-news').addClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
        
    })
    $('.nav-long__item-news').click(function(){
        $('.main').addClass('hidden');
        $('.home').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $(this).addClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('.nav-short__item-news').addClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.nav-short__item-news').click(function(){
        $('.main').addClass('hidden');
        $('.home').removeClass('hidden');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $(this).addClass('nav-short__item--active');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-long__item-news').addClass('nav-long__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })

    $('.nav-long__item-members').click(function(){
        $('.main').addClass('hidden');
        $('.members').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $(this).addClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('.nav-short__item-member').addClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.nav-short__item-member').click(function(){
        $('.main').addClass('hidden');
        $('.members').removeClass('hidden');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $(this).addClass('nav-short__item--active');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-long__item-members').addClass('nav-long__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.nav-long__item-blog').click(function(){
        $('.main').addClass('hidden');
        $('.blog').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $(this).addClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('.nav-short__item-blog').addClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.nav-short__item-blog').click(function(){
        $('.main').addClass('hidden');
        $('.blog').removeClass('hidden');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $(this).addClass('nav-short__item--active');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-long__item-blog').addClass('nav-long__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.react-box__title-all').click(function(){
        $('.active-blog').removeClass('active-blog');
        $(this).addClass('active-blog');
        $('.users-box').not('.users-box__show').addClass('users-box__show');
        $('.users-box-all').removeClass('users-box__show');
    })
    $('.react-box__title-love').click(function(){
        $('.active-blog').removeClass('active-blog');
        $(this).addClass('active-blog');
        $('.users-box').not('.users-box__show').addClass('users-box__show');
        $('.users-box-love').removeClass('users-box__show');
    })
    $('.react-box__title-care').click(function(){
        $('.active-blog').removeClass('active-blog');
        $(this).addClass('active-blog');
        $('.users-box').not('.users-box__show').addClass('users-box__show');
        $('.users-box-care').removeClass('users-box__show');
    })
    $('.react-box__title-like').click(function(){
        $('.active-blog').removeClass('active-blog');
        $(this).addClass('active-blog');
        $('.users-box').not('.users-box__show').addClass('users-box__show');
        $('.users-box-like').removeClass('users-box__show');
    })
    $('.content-action-active').each(function(index,element){
        element.onclick = function(){
            $(this).children('.react-modal').toggleClass('class-display-flex');
        }
    })
    $('.react-box').click(function(e){
        e.stopPropagation();
    })
    $('.react-modal__quite').click(function(e){
        // $('.react-modal').addClass('test');
        e.stopPropagation();
        $('.react-modal').removeClass('class-display-flex');
    })
    // lọc của trang new
    var a = $('.quick-filters-form').children('select');
    a.change(function(){
        if($(this).val()=='Everything'){
            $('.home .show-news').removeClass('show-news');
            $('.home .widget-box').css('animation','fade 0.5s ease-in');
        };
        if($(this).val()=='Status'){
            $('.home .show-news').removeClass('show-news');
            $('.home .widget-box').not('.statuss').addClass('show-news');
            $('.home .widget-box').css('animation','fade 0.5s ease-in');
        };
        if($(this).val()=='Share'){
            $('.home .show-news').removeClass('show-news');
            $('.home .widget-box').not('.share').addClass('show-news');
            $('.home .widget-box').css('animation','fade 0.5s ease-in');
        };
        if($(this).val()=='Media'){
            $('.home .show-news').removeClass('show-news');
            $('.home .widget-box').not('.media').addClass('show-news');
            $('.home .widget-box').css('animation','fade 0.5s ease-in');
        };
    });
    // page của trang blog
    $('.page-blog__number-1').click(function(){
        $('.page-blog__number-current').removeClass('page-blog__number-current');
        $(this).addClass('page-blog__number-current');
        $('.page-content-1').removeClass('show-blog');
        $('.page-content-2').addClass('show-blog');
    });
    $('.page-blog__number-2').click(function(){
        $('.page-blog__number-current').removeClass('page-blog__number-current');
        $(this).addClass('page-blog__number-current');
        $('.page-content-2').removeClass('show-blog');
        $('.page-content-1').addClass('show-blog');
    });
    $('.next').click(function(){
        $('.page-blog__number-current').removeClass('page-blog__number-current');
        $('.page-blog__number-2').addClass('page-blog__number-current');
        $('.page-content-2').removeClass('show-blog');
        $('.page-content-1').addClass('show-blog');
    });
    // Chuyển đổi các tab trang chi tiết cá nhân
    $('.section-navigation-slider-box__about').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $(this).addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.about').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.section-navigation-slider-box__timeline').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $(this).addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.timeline').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.section-navigation-slider-box__friends').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $(this).addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.friends').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.section-navigation-slider-box__groups').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $(this).addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.groups2').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.section-navigation-slider-box__photos').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $(this).addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.photos').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.picture-item-wrap').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $('.section-navigation-slider-box__photos').addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.photos').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
      $('.widget-box-button').click(function(){
        $('.section-navigation-slider-active').removeClass('section-navigation-slider-active');
        $('.section-navigation-slider-box__groups').addClass('section-navigation-slider-active');
        $('.member-detail').not('.show-hide').addClass('show-hide');
        $('.groups').removeClass('show-hide');
        $('.member-detail').css('animation','fade 0.5s ease-in');
        
      })
    //   hiện trang chi tiết các nhân
    $(".list-members__info").click(function(e){
        e.preventDefault();
        $('.main').addClass('hidden');
        $('.include-members-detail').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $(".list-members__avata-link").click(function(e){
        e.preventDefault();
        $('.main').addClass('hidden');
        $('.include-members-detail').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $(".nav-short__item").click(function(){
        $(".include-members-detail").addClass('hidden');
    })
    $(".nav-long__item").click(function(){
        $(".include-members-detail").addClass('hidden');
    })
    // Hiện trang chi chi tiết bài viết
    $(".post-preview-link").click(function(){
        $('.main').addClass('hidden');
        $('.blog-detail').removeClass('hidden');
    })
    $(".nav-short__item").click(function(){
        $(".blog-detail").addClass('hidden');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $(".nav-long__item").click(function(){
        $(".blog-detail").addClass('hidden');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    // Trang group 
    $('.nav-short__item-groups').click(function(){
        $('.main').addClass('hidden');
        $('.groups').removeClass('hidden');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $(this).addClass('nav-short__item--active');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $('.nav-long__item-groups').addClass('nav-long__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    $('.nav-long__item-groups').click(function(){
        $('.main').addClass('hidden');
        $('.groups').removeClass('hidden');
        $('.nav-long__item').removeClass('nav-long__item--active');
        $(this).addClass('nav-long__item--active');
        $('.nav-short__item').removeClass('nav-short__item--active');
        $('.nav-short__item-groups').addClass('nav-short__item--active');
        $('html').animate({
            scrollTop : 0
       },800)
    })
    // Lọc, sắp xếp tìm kiếm trang groups

    // Focus Input
    $('.groups .groups-filters__input').focus(function(){
        $('.groups .groups-filters__placeholder').css('top','-14px');
    })
    $('.groups .groups-filters__input').focusout(function() {
        if( !$(this).val())
            $('.groups-filters__placeholder').css('top','8px');
    })

    // Seach
    $('.groups .groups-filters__input').on("keyup", function(){
        var values = $(this).val().toLowerCase();
        $('.groups .list-groups__member').parent().filter(function(){
            $(this).toggle($(this).children().children('.list-groups__info').text().toLowerCase().indexOf(values) > -1);
            
            if( $(this).children().children('.list-groups__info').text().toLowerCase().indexOf(values) > -1)
            {
                $(this).unhighlight();
                $(this).children().children('.list-groups__info').highlight(values);
                return true;
            }
            else
                return false;
        });
    })
    // Sort
    var mylist2 = $('.groups .list-groups__name-link');
    var listitems2 = mylist2.children('.groups .list-groups__name');
    listitems2.sort(function(a, b) {
        return $(a).text().toUpperCase().localeCompare($(b).text().toUpperCase());
    })
    listitems2.each(function(){
        $('.groups .list-groups').append($(this).parent().parent().parent().parent())
    })
    $('.groups .groups-filters__section').change(function(){
        var chose2 = $(this).find(":selected").val();
        if (chose2 == 1) {
            var mylist3 = $('.groups .list-groups__name-link');
            var listitems = mylist3.children('.groups .list-groups__name');
            listitems.sort(function(a, b) {
                return $(a).text().toUpperCase().localeCompare($(b).text().toUpperCase());
            })
            listitems.each(function(){
                $('.groups .list-groups').append($(this).parent().parent().parent().parent())
            })
        }
        if (chose2 == 2) {
            var mylist3 = $('.groups .list-groups__post');
            var listitems = mylist3.children('.groups .posts-numbers');
            listitems.sort(function(a, b) {
                return + $(b).text() - + $(a).text();
            })
            listitems.each(function(){
                $('.groups .list-groups').append($(this).parent().parent().parent().parent())
            })
        }
        if (chose2 == 3) {
            var mylist3 = $('.groups .list-groups__post');
            var listitems = mylist3.children('.groups .number-friend');
            listitems.sort(function(a, b) {
                return + $(b).text() - + $(a).text();
            })
            listitems.each(function(){
                $('.groups .list-groups').append($(this).parent().parent().parent().parent())
            })
        }
        
    })
    // Lưới
    $('.groups-filters__grids-icon').click(function(){
        $('.groups-filters__grids-icon').removeClass('active');
        $(this).addClass('active');
    })
    $('.big-grid').click(function(){
        $('.groups .list-groups').children().addClass('col-lg-4');
    })
    $('.small-grid').click(function(){
        $('.groups .list-groups').children().removeClass();
        $('.groups .list-groups').children().addClass('col-lg-3');
    })
    // scroll header
    window.addEventListener("scroll",function(){
        if(self.pageYOffset>50)
        {
            $('.btn-top').css('display','flex');
        }
        else{
            $('.btn-top').css('display','none');
        }
    
    })
    $('.btn-top').click(function(){
         $('html').animate({
             scrollTop : 0
        },800)
    })
})




    



